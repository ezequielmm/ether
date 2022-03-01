using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;
using UnityEngine.EventSystems;

public class LoginPanelManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    [Space(20)] public TMP_Text validEmailLabel;
    public TMP_Text validLoginEmail;
    public TMP_Text validLoginPassword;

    [Space(20)] public Toggle rememberMe;
    public Button loginButton;

    [Space(20)] public WebRequesterManager webRequesterManager;

    private bool validEmail;
    private bool validLogin;

    private void Start()
    {
        validEmailLabel.gameObject.SetActive(false);
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);

        loginButton.interactable = false;
    }

    private void Update()
    {
        loginButton.interactable = validEmail && !string.IsNullOrEmpty(passwordInputField.text);
    }

    public void VerifyEmail()
    {
        validLoginEmail.gameObject.SetActive(false);

        validEmail = ParseString.IsEmail(emailInputField.text);
        validEmailLabel.gameObject.SetActive(!validEmail);
    }

    public void OnLogin()
    {
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);

        webRequesterManager.RequestLogin(emailInputField.text, passwordInputField.text, rememberMe.isOn, ValidateLogin);
    }

    private void ValidateLogin(bool validLoginLocal)
    {
        validLogin = validLoginLocal;

        if (validEmail)
        {
            if (validLogin)
            {
                gameObject.SetActive(false);
            }
            else
            {
                emailInputField.text = "";
                passwordInputField.text = "";

                validLoginEmail.gameObject.SetActive(!validLogin);
                validLoginPassword.gameObject.SetActive(!validLogin);
            }
        }
    }
}