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

    private void Awake()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSucessful);
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.AddListener(OnLoginError);
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerLoginPanel);
    }

    private void OnLoginSucessful(string userName, int fiefAmount)
    {
        if (rememberMe.isOn)
        {
            PlayerPrefs.SetString("user_email", emailInputField.text);
            PlayerPrefs.Save();
        }

        ActivateInnerLoginPanel(false);
    }

    private void OnLoginError(string errorMessage)
    {
        validLoginPassword.gameObject.SetActive(true);
        passwordInputField.text = "";
        Debug.Log("-------------------Login Error------------------");
    }

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

        if (validEmail)
        {
           GameManager.Instance.EVENT_REQUEST_LOGIN.Invoke(emailInputField.text, passwordInputField.text);
        }
    }
       

    public void ActivateInnerLoginPanel(bool activate)
    {
        loginContainer.SetActive(activate);
   
    }
}