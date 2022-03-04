using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText;

    public Button playButton, treasuryButton, registerButton, loginButton;

    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSucessful);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        TooglePreLoginStatus(true);
    }

    public void OnLoginSucessful(string name, int fief)
    {
        nameText.text = name;
        moneyText.text = fief.ToString();

        TooglePreLoginStatus(false);

    }

    public void TooglePreLoginStatus(bool preLoginStatus)
    {
        nameText.gameObject.SetActive(!preLoginStatus);
        moneyText.gameObject.SetActive(!preLoginStatus);
        playButton.interactable = !preLoginStatus;
        treasuryButton.interactable = !preLoginStatus;
        registerButton.gameObject.SetActive(preLoginStatus);
        loginButton.gameObject.SetActive(preLoginStatus);
    }

    public void OnRegisterButton()
    {
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnLoginButton()
    {
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }
}