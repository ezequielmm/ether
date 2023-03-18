using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// Check HelperClasses.cs for the classes usaed to hold JSON data
/// </summary>
public class WebRequesterManager : SingleTon<WebRequesterManager>
{
    private string baseUrl => ClientEnvironmentManager.Instance.WebRequestURL;

    protected override void Awake()
    {
        base.Awake();

        if (this != Instance) return;

        PlayerPrefs.SetString("api_url", baseUrl);

        SetSessionToken(string.Empty);

        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener(OnRegisterEvent);
        GameManager.Instance.EVENT_REQUEST_LOGIN.AddListener(RequestLogin);
        GameManager.Instance.EVENT_REQUEST_PROFILE.AddListener(RequestProfile);
        GameManager.Instance.EVENT_REQUEST_LOGOUT.AddListener(RequestLogout);
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.AddListener(RequestExpeditionCancel);
        GameManager.Instance.EVENT_SEND_BUG_FEEDBACK.AddListener(SendBugReport);
    }

    private void SetSessionToken(string token) 
    {
        PlayerPrefs.SetString("session_token", token);
        PlayerPrefs.Save();
    }

    public void RequestLogout(string token)
    {
        StartCoroutine(GetLogout(token));
    }

    public void RequestExpeditionStatus()
    {
        StartCoroutine(GetExpeditionStatus());
    }

    public void RequestExpeditionCancel()
    {
        StartCoroutine(CancelOngoingExpedition());
    }

    public void OnRegisterEvent(string name, string email, string password)
    {
        StartCoroutine(GetRegister(name, email, password));
    }

    public void RequestLogin(string email, string password)
    {
        StartCoroutine(GetLogin(email, password));
    }

    public void RequestProfile(string token)
    {
        StartCoroutine(GetProfile(token));
    }

    public void RequestCharacterList()
    {
        StartCoroutine(GetCharacterList());
    }

    public async UniTask<DownloadHandler> MakeRequest(UnityWebRequest request) 
    {
#if UNITY_EDITOR
        if (UnitTestDetector.IsInUnitTest)
        {
            Debug.Log($"[WebRequesterManager] Can't make a webrequest while testing.");
            return null;
        }
#endif
        try
        {
            Guid requestId = Guid.NewGuid();
            LogRequest(requestId, request.uri.ToString());
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception();
            }
            else
            {
                bool logText = IsResponseJson(request) && request?.downloadHandler?.text != null;
                LogRepsonse(requestId, request.uri.ToString(), logText ? request.downloadHandler.text : 
                    $"<Cannot Display: {request.GetResponseHeader("Content-Type")}>");
                return request.downloadHandler;
            }
        }
        catch(Exception e) 
        {
            Debug.LogException(e);
            Debug.LogError($"[WebRequesterManager] Error sending [{request.method}] request to [{request.uri}]\n{request?.error}");
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.method}][{request.uri}] Data Not Retrieved: {request?.error}", CommunicationDirection.Incoming);
            return null;
        }
    }

    List<string> jsonCarryingFormats = new List<string>() { "application/json", "text/plain", "text/json" };
    private bool IsResponseJson(UnityWebRequest response)
    {
        string responseType = response.GetResponseHeader("Content-Type")?.ToLower().Split(';')?
            .GetValue(0)?.ToString() ?? string.Empty;
        return jsonCarryingFormats.Contains(responseType);
    }

    private void LogRequest(Guid requestId, string url, params object[] payload) 
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
        string variableString = LogHelper.VariablesToHumanReadable(url, payload);
        string jsonString = LogHelper.VariablesToJson(url, payload);
        Debug.Log($"[WebRequesterManager] REQUEST [{requestIdShortened}] >>> {jsonString}");
        LogHelper.SendOutgoingCommunicationLogs($"[WebRequesterManager] REQUEST [{requestIdShortened}] >>> {variableString}", jsonString);
    }

    private void LogRepsonse(Guid requestId, string url, string rawJson)
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
        string variableString = LogHelper.VariablesToHumanReadable(url, rawJson);
        Debug.Log($"[WebRequesterManager] RESPONSE [{requestIdShortened}] <<< {rawJson}");
        LogHelper.SendOutgoingCommunicationLogs($"[WebRequesterManager] RESPONSE [{requestIdShortened}] <<< {variableString}", rawJson);
    }



    /// <summary>
    /// GetRegister
    /// </summary>
    /// <param name="nameText"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public IEnumerator GetRegister(string nameText, string email, string password)
    {
        string registerUrl = $"{baseUrl}{RestEndpoint.Register}";
        WWWForm form = new WWWForm();
        form.AddField("name", nameText);
        form.AddField("email", email);
        form.AddField("email_confirmation", email);
        form.AddField("password", password);
        form.AddField("password_confirmation", password);

        ServerCommunicationLogger.Instance.LogCommunication(
            $"Registration requested. username: {nameText} email: {email}", CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Post(registerUrl, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Register error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication(
                $"Registration Success. {request.downloadHandler.text}", CommunicationDirection.Incoming);
            RegisterData registerData = JsonConvert.DeserializeObject<RegisterData>(request.downloadHandler.text);
            string token = registerData.data.token;

            Debug.Log("Registration sucessful, token is " + token);

            //TO DO: check for errors even on a sucessful answer

            GameManager.Instance.EVENT_REQUEST_PROFILE
                .Invoke(token); //we request the profile to confirm the server got our account created properly. This will invoke later EVENT_LOGIN_COMPLETED
        }
    }

    /// <summary>
    /// GetLogin
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="rememberMe"></param>
    /// <returns></returns>
    IEnumerator GetLogin(string email, string password)
    {
        string loginUrl = $"{baseUrl}{RestEndpoint.Login}";

        Debug.Log("Login url:" + loginUrl);
        ServerCommunicationLogger.Instance.LogCommunication($"Login Requested. email: {email}",
            CommunicationDirection.Outgoing);
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Login error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke(request.error);
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication($"Login success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
            LoginData loginData = JsonConvert.DeserializeObject<LoginData>(request.downloadHandler.text);
            string token = loginData.data.token;

            //TODO: check for errors even on successful result

            GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token);
        }
    }

    IEnumerator GetProfile(string token)
    {
        // Debug.Log("Getting profile with token " + token);

        string profileUrl = $"{baseUrl}{RestEndpoint.Profile}";
        ServerCommunicationLogger.Instance.LogCommunication("Profile request. token: " + token,
            CommunicationDirection.Outgoing);
        //UnityWebRequest profileInfoRequest = UnityWebRequest.Get($"{profileUrl}?Authorization={Uri.EscapeDataString(token)}");
        using (UnityWebRequest
               request = UnityWebRequest
                   .Get(profileUrl))
        {
            // TO DO: this should be asking for authorization on the header
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error getting profile with token " + token);
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"Profile error. token: {token}, error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                yield break;
            }

            //TODO: check for errors even when the result is successful

            ServerCommunicationLogger.Instance.LogCommunication(
                $"Request profile successful: {request.downloadHandler.text}", CommunicationDirection.Incoming);
            ProfileData profileData = JsonConvert.DeserializeObject<ProfileData>(request.downloadHandler.text);
            string name = profileData.data.name;
            int fief = profileData.data.fief;

            SetSessionToken(token);

            RequestExpeditionStatus();

            GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL
                .Invoke(profileData); //TODO: these 2 events here don't look good
            GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke(name, fief);
        }
    }

    IEnumerator GetLogout(string token)
    {
        string loginUrl = $"{baseUrl}{RestEndpoint.Logout}";
        WWWForm form = new WWWForm();

        ServerCommunicationLogger.Instance.LogCommunication($"Logout request. token: {token}",
            CommunicationDirection.Outgoing);
        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Logout request error: {request.error}",
                    CommunicationDirection.Incoming);
                GameManager.Instance.EVENT_REQUEST_LOGOUT_ERROR.Invoke(request.error);
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication($"Logout Success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
            LogoutData logoutData = JsonConvert.DeserializeObject<LogoutData>(request.downloadHandler.text);
            string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result

            GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.Invoke(message);
        }
    }

    IEnumerator GetExpeditionStatus()
    {
        string token = PlayerPrefs.GetString("session_token");

        //Debug.Log("[RequestExpeditionStattus] with token " + token);

        string fullUrl = $"{baseUrl}{RestEndpoint.ExpeditionStatus}";
        WWWForm form = new WWWForm();
        ServerCommunicationLogger.Instance.LogCommunication($"Expedition Status request. token: {token}",
            CommunicationDirection.Outgoing);
        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            // using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
            request.SetRequestHeader("Accept", "*/*");
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            // Debug.Log(request.GetRequestHeader("Authorization"));

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Expedition status error: {request.error}",
                    CommunicationDirection.Incoming);

                Debug.Log("[Error getting expedition status] " + request.error);

                yield break;
            }

            ExpeditionStatus data = JsonConvert.DeserializeObject<ExpeditionStatus>(request.downloadHandler.text);

            GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(data.data);

            Debug.Log("[WebRequestManager] Expedition status " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                $"Expedition status success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
        }
    }

    IEnumerator GetCharacterList()
    {
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("[GetCharacterList] with token " + token);
        ServerCommunicationLogger.Instance.LogCommunication("Character list request. token: " + token,
            CommunicationDirection.Outgoing);

        string fullUrl = $"{baseUrl}{RestEndpoint.CharactersList}";
        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Get($"{fullUrl}?Authorization={Uri.EscapeDataString(token)}"))
        {
            // using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error getting GetCharacterList status] " + request.error);
                ServerCommunicationLogger.Instance.LogCommunication(
                    "Character list error: " + request.error, CommunicationDirection.Incoming);

                yield break;
            }

            Debug.Log("answer from GetCharacterList status " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "Character list success: " + request.downloadHandler.text, CommunicationDirection.Incoming);


            //LogoutData answerData = JsonConvert.DeserializeObject<LogoutData>(request.downloadHandler.text);
            //string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result
        }
    }

    private IEnumerator CancelOngoingExpedition()
    {
        string fullURL = $"{baseUrl}{RestEndpoint.ExpeditionCancel}";
        string token = PlayerPrefs.GetString("session_token");

        ServerCommunicationLogger.Instance.LogCommunication($"Expedition cancel request. token: {token}",
            CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Post(fullURL, ""))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error canceling expedition]");
                ServerCommunicationLogger.Instance.LogCommunication("Expedition cancel error: " + request.error,
                    CommunicationDirection.Outgoing);
                yield break;
            }

            GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(new ExpeditionStatusData
            {
                hasExpedition = false,
                nftId = -1
            });
            Debug.Log("answer from cancel expedition " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "cancel expedition success: " + request.downloadHandler.text, CommunicationDirection.Outgoing);
        }
    }

    private async void SendBugReport(string title, string description, string base64Image)
    {
        string fullUrl = $"{baseUrl}{RestEndpoint.BugReport}";

        BugReportData reportData = new BugReportData
        {
            reportId = Guid.NewGuid().ToString(),
            environment = ClientEnvironmentManager.Instance.Environment.ToString(),
            clientId = UserDataManager.Instance.ClientId,
            account = UserDataManager.Instance.UserAccount,
            knightId = UserDataManager.Instance.ActiveNft,
            expeditionId = UserDataManager.Instance.ExpeditionId,
            userTitle = title,
            userDescription = description,
            screenshot = base64Image,
            frontendVersion = VersionManager.ClientVersionWithCommit,
            backendVersion = VersionManager.ServerVersion,
            messageLog = ServerCommunicationLogger.Instance.GetCommunicationLog()
        };
        string data = JsonConvert.SerializeObject(reportData);
        byte[] utf8String = Encoding.Default.GetBytes(data);
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            var uploadHandler = new UploadHandlerRaw(utf8String);
            uploadHandler.contentType = $"application/json";
            request.uploadHandler = uploadHandler;
            await MakeRequest(request);
            uploadHandler.Dispose();
        }
        Debug.Log($"[WebRequesterManager] Bug Report Sent!");
    }

    public string ConstructUrl(string path) 
    {
        string host = ClientEnvironmentManager.Instance.WebRequestURL;
        //TODO TEMP CODE UNTIL SERVER UPDATES
        if (path == RestEndpoint.WalletData) host = ClientEnvironmentManager.Instance.WebSocketURL;
        return $"{host}{path}";
    }
}


public static class RestEndpoint 
{
    public static readonly string RandomName = "/auth/v1/generate/username";
    public static readonly string Register = "/auth/v1/register";
    public static readonly string Login = "/auth/v1/login";
    public static readonly string Logout = "/auth/v1/logout";
    public static readonly string Profile = "/gsrv/v1/profile";
    //public static readonly string WalletData = "/gsrv/v1/wallets"; TODO restore when merged to main
    public static readonly string WalletData = "/v1/wallets";
    public static readonly string VerifyWalletSignature = "/gsrv/v1/tokens/verify";
    public static readonly string CharactersList = "/gsrv/v1/characters";
    public static readonly string ExpeditionStatus = "/gsrv/v1/expeditions/status";
    public static readonly string ExpeditionRequest = "/gsrv/v1/expeditions";
    public static readonly string ExpeditionCancel = "/gsrv/v1/expeditions/cancel";
    public static readonly string ExpeditionScore = "/gsrv/v1/expeditions/score";
    public static readonly string BugReport = "/gsrv/v1/bug/report";
    public static readonly string ServerVersion = "/gsrv/v1/showversion";
    public static readonly string PlayerGear = "/gsrv/v1/playergear";
}