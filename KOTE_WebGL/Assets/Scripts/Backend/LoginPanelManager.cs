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

    [Space(20)] public GameObject loginContainer;

    private bool validEmail;
    private bool validLogin;

    private void Start()
    {
        validEmailLabel.gameObject.SetActive(false);
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);

        loginButton.interactable = false;

        GameManager.Instance.EVENT_LOGIN_COMPLETED.AddListener(ValidateLogin);
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerLoginPanel);
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

        GameManager.Instance.EVENT_LOGIN.Invoke(emailInputField.text, passwordInputField.text, rememberMe.isOn);
    }

    private void ValidateLogin(string token, bool validLoginLocal)
    {
        validLogin = validLoginLocal;

        if (validEmail)
        {
            if (validLogin)
            {
                gameObject.SetActive(false);

                if (rememberMe.isOn)
                {
                    PlayerPrefs.SetString("auth_token", token);
                    PlayerPrefs.Save();
                }
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

    public void ActivateInnerLoginPanel(bool activate)
    {
        loginContainer.SetActive(activate);
    }
}