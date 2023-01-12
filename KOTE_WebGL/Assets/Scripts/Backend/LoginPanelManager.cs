using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;
using UnityEngine.EventSystems;

public class LoginPanelManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    [Space(20)]
    public TMP_Text validEmailLabel;

    public TMP_Text validLoginEmail;
    public TMP_Text validLoginPassword;

    [Space(20)]
    public Toggle rememberMe;

    public Toggle showPassword;
    public Button loginButton;

    [Space(20)]
    public GameObject loginContainer;

    private bool validEmail;
    private bool validLogin;

    private void Awake()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSucessful);
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.AddListener(OnLoginError);
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerLoginPanel);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        string email = PlayerPrefs.GetString("email_reme_login");
        string date = PlayerPrefs.GetString("date_reme_login");

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(date))
        {
            float days = (float) (DateTime.ParseExact(date, "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture) - DateTime.Today).TotalDays;

            if (!(Mathf.Abs(days) >= 15))
            {
                emailInputField.text = email;
                VerifyEmail();
                rememberMe.isOn = true;
            }
        }
    }

    public void OnShowPassword()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        passwordInputField.contentType = showPassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
    }

    public void OnRegisterButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ActivateInnerLoginPanel(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    private void OnLoginSucessful(string userName, int fiefAmount)
    {
        if (rememberMe.isOn)
        {
            PlayerPrefs.SetString("email_reme_login", emailInputField.text);
            PlayerPrefs.SetString("date_reme_login", DateTime.Today.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        ActivateInnerLoginPanel(false);

        if (MetaMaskAdapter.Instance.HasMetamask())
        {
            MetaMaskAdapter.Instance.RequestWallet();
        }
    }

    private void OnLoginError(string errorMessage)
    {
        validLoginPassword.gameObject.SetActive(true);
        passwordInputField.text = "";

        PlayerPrefs.DeleteKey("email_reme_login");
        PlayerPrefs.DeleteKey("date_reme_login");
        PlayerPrefs.Save();

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
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);

        if (validEmail)
        {
            GameManager.Instance.EVENT_REQUEST_LOGIN.Invoke(emailInputField.text, passwordInputField.text);
        }
    }

    public void OnLogoutSuccessful(string token)
    {
        passwordInputField.text = "";
    }

    public void ActivateInnerLoginPanel(bool activate)
    {
        loginContainer.SetActive(activate);
    }
}