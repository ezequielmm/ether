using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Papertrail
{
    /// <summary>
    /// Fowards Unity logs to Papertrail's monitoring servers using the syslog protocol.
    /// </summary>
    public class PapertrailLogger : MonoBehaviour
    {
        // Format for messages that use a tag
        private const string s_taggedNoStack = "tag=[{0}] {1}";

        // Format for messages that use a tag
        private const string s_logFormatNoStack = "{0}";

        // Singleton instance of the PapertrailLogger
        public static PapertrailLogger Instance
        {
            get
            {
                if (s_instance == null)
                {
                    Initialize();
                }

                return s_instance;
            }
        }

        // Private singleton instnace storage
        private static PapertrailLogger s_instance;

        // Papertrail logging settings
        //private PapertrailSettings m_settings;

        // Builds messages with minimal garbage allocations
        private StringBuilder m_stringBuilder = new StringBuilder();

        // The clients external IP address
        private string m_localIp;

        // Flag set when the client is connected and ready to being logging
        private bool m_isReady;

        // Flag set when any scene is being loaded so there is an active thread
        private bool m_isLoaded;

        // Log messages are queued up until the client is ready to log
        private Queue<string> m_queuedMessages = new Queue<string>();

        // User set tag for log messages
        private string m_tag;


        //minimum logging level
        private Severity minimumLoggingLevel = Severity.Debug;

        // Default facility tag to use for logs.
        private Facility facility = Facility.local7;

        /// <summary>
        /// Initializes the logging instance as soon as the app starts
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<PapertrailLogger>();
                if (s_instance == null)
                {
                    s_instance = new GameObject("PapertrailLogger").AddComponent<PapertrailLogger>();
                }

                DontDestroyOnLoad(s_instance.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Debug.unityLogger.logHandler.GetType() == typeof(PapertrailLogHandler)) 
            {
                ((PapertrailLogHandler)Debug.unityLogger.logHandler).Dispose();
            }
        }

        /// <summary>
        /// Called when the Instance is created. Gathers application information and creates the UDP client
        /// </summary>
        private void Awake()
        {
            // Ensure this is the only instance
            if (s_instance != null && s_instance != this)
            {
                Destroy(this);
                return;
            }

            // Load settings
            m_isReady = false;
            m_localIp = "0.0.0.0";
            if (SceneManager.GetActiveScene().isLoaded) m_isLoaded = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            HiddenConsoleManager.DisableOnBuild();
            Debug.unityLogger.logHandler = new PapertrailLogHandler(Debug.unityLogger.logHandler);
            GameManager.Instance.EVENT_SCENE_LOADING.AddListener(OnLoadScene);

            StartCoroutine(GetExternalIP());
        }


        // so the message are queued to not send when a scene is loading
        private void OnLoadScene()
        {
            m_isLoaded = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu" || scene.name == "Expedition")
            {
                m_isLoaded = true;
                if (m_isReady) SendQueuedMessages();
            }
        }

        /// <summary>
        /// Retrieves the external IP address of the client to append to log messages.
        /// Waits until an internet connection can be established before starting logs.
        /// </summary>
        private IEnumerator GetExternalIP()
        {
            // Wait for an internet connection
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return new WaitForSeconds(1);
            }

            int maxRetry = 3;
            while (maxRetry > 0)
            {
                maxRetry--;
                // Find the client's external IP address
                using (UnityWebRequest webRequest = UnityWebRequest.Get("https://api.ipify.org?format=text"))
                {
                    yield return webRequest.SendWebRequest();
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        m_localIp = webRequest.downloadHandler.text;
                        break;
                    }
                }
                yield return new WaitForSeconds(1);
            }

            // Set the logger as ready to send messages
            m_isReady = true;
            Debug.Log("Papertrail Logger Initialized");

            if (m_isLoaded) StartCoroutine(SendQueuedMessages());
        }

        private IEnumerator SendQueuedMessages()
        {
            // Send all messages that were waiting for initialization
            while (m_queuedMessages.Count > 0)
            {
                BeginSend(m_queuedMessages.Dequeue());
                yield return null;
            }
        }

        /// <summary>
        /// Callback for the Unity logging system. Happens off of the main thread.
        /// </summary>
        public void Application_LogMessageReceived(string condition, LogType type)
        {
            // Set the severity type based on the Unity's log level
            Severity severity = Severity.Debug;
            switch (type)
            {
                case LogType.Assert:
                    severity = Severity.Alert;
                    break;
                case LogType.Exception:
                    severity = Severity.Critical;
                    break;
                case LogType.Error:
                    severity = Severity.Error;
                    break;
                case LogType.Log:
                    severity = Severity.Debug;
                    break;
                case LogType.Warning:
                    severity = Severity.Warning;
                    break;
            }

            try
            {
                if (severity > minimumLoggingLevel)
                    return;
                if (!string.IsNullOrEmpty(m_tag))
                {
                    Log(severity, string.Format(s_taggedNoStack, m_tag, condition));
                }
                else
                {
                    Log(severity, string.Format(s_logFormatNoStack, condition));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Begin sending a message asynchrously on the UDP client
        /// </summary>
        private void BeginSend(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            if (!m_isReady || !m_isLoaded)
            {
                // Enqueue messages if the logger isn't fully initialized
                m_queuedMessages.Enqueue(msg);
                return;
            }

            StartCoroutine(LogWebRequest(msg));
        }

        /// <summary>
        /// sends off the message to papertrail. This is fire-and-forget, so we don't need to have it in a coroutine
        /// </summary>
        /// <param name="msg">message to be logged</param>
        private IEnumerator LogWebRequest(string msg)
        {
#if UNITY_EDITOR
            if (UnitTestDetector.IsInUnitTest)
            {
                yield break;
            }
#endif
            using (UnityWebRequest request =
                   new UnityWebRequest("https://logs.collector.solarwinds.com/v1/log", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(msg));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Basic OnBvMWtBNjNrZktTT1ptejNYYTNHT1RQTDdXbzY=");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    //Debug.LogError("Logging data failed " + request.error);
                }
            }
        }

        /// <summary>
        /// Internal instance logging of a message
        /// </summary>
        private void LogInternal(string msg)
        {
            Log(facility, Severity.Debug, msg);
        }

        /// <summary>
        /// Internal instance logging of a message
        /// </summary>
        private void LogInternal(Severity severity, string msg)
        {
            Log(facility, severity, msg);
        }

        /// <summary>
        /// Internal instance logging of a message
        /// </summary>
        private void LogInternal(Facility facility, Severity severity, string msg)
        {
            return;
            // Early out if the client's logging level is lower than the log message
            if (string.IsNullOrEmpty(msg) || severity > minimumLoggingLevel)
                return;
            // Calculate the message severity (facility * 8 + severity)
            int severityValue = ((int)facility) * 8 + (int)severity;
            string message = string.Empty;

            PapertrailLogData logData = new PapertrailLogData(severityValue, msg);
            message = JsonConvert.SerializeObject(logData);

            // Send the completed message
            BeginSend(message);
        }

        /// <summary>
        /// Set a user tag to be appended to all outgoing logs
        /// </summary>
        /// <param name="tag">Tag that will appended to all outgoing messages</param>
        public static void SetTag(string tag)
        {
            Instance.m_tag = tag;
        }

        /// <summary>
        /// Log a message to the remote server
        /// </summary>
        /// <param name="msg">Message to be logged</param>
        public static void Log(string msg)
        {
            Instance.LogInternal(Severity.Debug, msg);
        }

        /// <summary>
        /// Log a message to the remote server
        /// </summary>
        /// <param name="severity">Severity level of the message</param>
        /// <param name="msg">Message to be logged</param>
        public static void Log(Severity severity, string msg)
        {
            Instance.LogInternal(severity, msg);
        }

        /// <summary>
        /// Log a message to the remote server
        /// </summary>
        /// <param name="facility">The sending facility of the message. See syslog protocol for more information.</param>
        /// <param name="severity">Severity level of the message</param>
        /// <param name="msg">Message to be logged</param>
        public static void Log(Facility facility, Severity severity, string msg)
        {
            Instance.LogInternal(facility, severity, msg);
        }
    }
}

public class PapertrailLogData
{
    public string env;
    public int level;
    public string service;
    public string ip;
    public string clientId;
    public string account;
    public string expeditionId;
    public string context;
    public string message;
    public string serverVersion;
    public string clientVersion;

    public PapertrailLogData(int severityValue, string message)
    {
        // Environment data
        env = ClientEnvironmentManager.Instance.Environment.ToString();
        level = severityValue;
        service = "Frontend";
        //ip = m_localIp;
        clientId = GetClientId();
        account = GetAccount();
        expeditionId = GetExpeditionId();
        serverVersion = VersionManager.ServerVersion;
        clientVersion = VersionManager.ClientVersionWithCommit;
        context = GetContext();

        this.message = message;
    }

    private string GetClientId()
    {
        return UserDataManager.Instance.ClientId;
    }

    private string GetAccount()
    {
        if (!string.IsNullOrEmpty(UserDataManager.Instance.Profile.UserAddress))
        {
            return UserDataManager.Instance.Profile.UserAddress;
        }
        return string.Empty;
    }

    private string GetExpeditionId()
    {
        if (!string.IsNullOrEmpty(UserDataManager.Instance.ExpeditionId))
        {
            return UserDataManager.Instance.ExpeditionId;
        }
        return string.Empty;
    }

    private const int FRAME_ROLLBACK = 10;
    private string GetContext() 
    {
        //MethodBase method = new System.Diagnostics.StackTrace().GetFrame(FRAME_ROLLBACK).GetMethod();
        //Type declaringType = method.DeclaringType;
        //if (declaringType == null)
        //{
        //    return method.Name;
        //}
        //return declaringType.FullName;
        return "unknown";
    }
}