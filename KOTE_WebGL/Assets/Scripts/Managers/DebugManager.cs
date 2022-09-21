using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public GameObject consoleContainer;
    public TMP_InputField consoleInput;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.AddListener(OnShowConsole);
        consoleContainer.SetActive(false);
        consoleInput.onSubmit.AddListener(OnTextInput);
    }

    /// <summary>
    /// Disables the logging if this is not a dev build
    /// </summary>
    public static void DisableOnBuild() 
    {
        #if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
            DisableDebug();
        #endif
    }
    /// <summary>
    /// Enables Logging
    /// </summary>
    public static void EnableDebug() 
    {
        Debug.unityLogger.filterLogType = LogType.Log;
    }
    /// <summary>
    /// Disables logging
    /// </summary>
    public static void DisableDebug()
    {
        Debug.unityLogger.filterLogType = LogType.Exception;
    }
    /// <summary>
    /// Allows one message to come through
    /// </summary>
    /// <param name="message">Message to log</param>
    public static void PublicLog(object message) 
    {
        var originalFilter = Debug.unityLogger.filterLogType;
        Debug.unityLogger.filterLogType = LogType.Log;
        Debug.Log(message);
        Debug.unityLogger.filterLogType = originalFilter;
    }


    private void OnShowConsole()
    {
        consoleContainer.SetActive(true);
    }

    public void OnTextInput(string input)
    {
        input = input.Trim().ToLower();
        consoleInput.text = "";
        switch (input)
        {
            case "ws_url":
                string url = PlayerPrefs.GetString("ws_url");
                if (url == "") url = "Websocket not yet connected.";
                PublicLog(url);
                break;
            case "apis_url":
            case "api_url":
                string
                    apiUrl = PlayerPrefs.GetString("api_url");
                if (apiUrl == "") url = "No API URL found.";
                PublicLog(apiUrl);
                break;
            case "player_token":
                string token = PlayerPrefs.GetString("session_token");
                if (token == "") token = "No token has been generated yet.";
                PublicLog(token);
                break;
            case "quit":
            case "exit":
                consoleContainer.SetActive(false);
                break;
            case "enable_debug":
                EnableDebug();
                PublicLog("Console Enabled.");
                break;
            case "disable_debug":
                DisableDebug();
                PublicLog("Console Disabled.");
                break;
            case "version":
                PublicLog(Application.version);
                break;
            default:
                PublicLog("Unknown Command.");
                break;
        }
    }
}