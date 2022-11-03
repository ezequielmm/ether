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
        bool isCommand = Enum.TryParse(input, out ConsoleCommands command);
        if (!isCommand)
        {
            PublicLog("Unknown Command.");
            return;
        }

        switch (command)
        {
            case ConsoleCommands.ws_url:
                string url = PlayerPrefs.GetString("ws_url");
                if (url == "") url = "Websocket not yet connected.";
                PublicLog(url);
                break;
            case ConsoleCommands.api_url:
            case ConsoleCommands.apis_url:
                string
                    apiUrl = PlayerPrefs.GetString("api_url");
                if (apiUrl == "") url = "No API URL found.";
                PublicLog(apiUrl);
                break;
            case ConsoleCommands.player_token:
                string token = PlayerPrefs.GetString("session_token");
                if (token == "") token = "No token has been generated yet.";
                PublicLog(token);
                break;
            case ConsoleCommands.quit:
            case ConsoleCommands.exit:
            case ConsoleCommands.close:
            case ConsoleCommands.close_console:
                consoleContainer.SetActive(false);
                break;
            case ConsoleCommands.enable_debug:
                EnableDebug();
                PublicLog("Console Enabled.");
                break;
            case ConsoleCommands.disable_debug:
                DisableDebug();
                PublicLog("Console Disabled.");
                break;
            case ConsoleCommands.version:
                PublicLog(Application.version);
                break;
            case ConsoleCommands.show_commands:
            case ConsoleCommands.help:
                ListCommands();
                break;
            case ConsoleCommands.enable_register_panel:
                PlayerPrefs.SetInt("enable_registration", 1);
                PublicLog("Register panel enabled.");
                break;
            case ConsoleCommands.enable_armory_panel:
                PlayerPrefs.SetInt("enable_armory", 1);
                PublicLog("Armory panel enabled.");
                break;
            case ConsoleCommands.enable_royal_houses_panel:
                PlayerPrefs.SetInt("enable_royal_house", 1);  
                PublicLog("Royal House panel enabled.");
                break;
            case ConsoleCommands.enable_node_numbers:
                PlayerPrefs.SetInt("enable_node_numbers", 1);
                PublicLog("Node numbers enabled");
                break;
            case ConsoleCommands.enable_all_functionality:
                PlayerPrefs.SetInt("enable_registration", 1);
                PlayerPrefs.SetInt("enable_armory", 1);
                PlayerPrefs.SetInt("enable_royal_house", 1);
                PublicLog("Register, armory, and royal house panels enabled");
                break;
            case ConsoleCommands.disable_register_panel:
                PlayerPrefs.SetInt("enable_registration", 0);
                PublicLog("Register panel disabled.");
                break;
            case ConsoleCommands.disable_armory_panel:
                PlayerPrefs.SetInt("enable_armory", 0);
                PublicLog("Armory panel disabled.");
                break;
            case ConsoleCommands.disable_royal_houses_panel:
                PlayerPrefs.SetInt("enable_royal_house", 0);
                PublicLog("Royal House panel disabled.");
                break;
            case ConsoleCommands.disable_node_numbers:
                PlayerPrefs.SetInt("enable_node_numbers", 0);
                PublicLog("Node numbers disabled");
                break;
            case ConsoleCommands.disable_all_functionality:
                PlayerPrefs.SetInt("enable_registration", 0);
                PlayerPrefs.SetInt("enable_armory", 0);
                PlayerPrefs.SetInt("enable_royal_house", 0);
                PublicLog("Register, armory, and royal house panels disabled");
                break;
            default:
                PublicLog("Unknown Command.");
                break;
        }
    }

    private void ListCommands()
    {
        string commandString = "Available commands: ";
        string[] commands = Enum.GetNames(typeof(ConsoleCommands));
        for (int i = 0; i < commands.Length; i++)
        {
            commandString += commands[i];
            if (i != commands.Length - 1)
            {
                commandString += ", ";
            }
        }

        PublicLog(commandString);
    }
}