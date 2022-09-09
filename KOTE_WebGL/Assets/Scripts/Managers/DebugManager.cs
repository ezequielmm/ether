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


    private void OnShowConsole()
    {
        consoleContainer.SetActive(true);
    }

    public void OnTextInput(string input)
    {
        input = input.Trim();
        consoleInput.text = "";
        switch (input)
        {
            case "ws_url":
                string url = PlayerPrefs.GetString("ws_url");
                if (url == "") url = "Websocket not yet connected";
                Debug.Log(url);
                break;
            case "apis_url":
            case "api_url":
                string
                    apiUrl = PlayerPrefs.GetString("api_url");
                if (apiUrl == "") url = "No API URL found";
                Debug.Log(apiUrl);
                break;
            case "player_token":
                string token = PlayerPrefs.GetString("session_token");
                if (token == "") token = "No token has been generated yet";
                Debug.Log(token);
                break;
            case "quit":
            case "exit":
                consoleContainer.SetActive(false);
                break;
        }
    }
}