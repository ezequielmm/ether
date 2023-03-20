using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    [Space(20)] public TMP_Text validEmailLabel;

    public TMP_Text validLoginEmail;
    public TMP_Text validLoginPassword;

    [Space(20)] public Toggle rememberMe;

    public Toggle showPassword;
    public Button loginButton;

    [Space(20)] public GameObject loginContainer;

    private bool validEmail;
    private bool validLogin;

    private void Awake()
    {
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerLoginPanel);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        string email = PlayerPrefs.GetString("email_reme_login");
        string date = PlayerPrefs.GetString("date_reme_login");

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(date))
        {
            float days = (float)(DateTime.ParseExact(date, "MM/dd/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture) - DateTime.Today).TotalDays;

            if (!(Mathf.Abs(days) >= 15))
            {
                emailInputField.text = email;
                VerifyEmail();
                rememberMe.isOn = true;
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Return) && loginButton.interactable && loginContainer.activeSelf)
        {
            OnLogin();
        }
    }

    public void OnShowPassword()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        passwordInputField.contentType =
            showPassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
    }

    public void OnRegisterButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ActivateInnerLoginPanel(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void RememberAndCloseLoginPanel()
    {
        if (rememberMe.isOn)
            RememberLoginInfo();
        else
            ForgetLoginInfo();
        ActivateInnerLoginPanel(false);
        validLoginPassword.gameObject.SetActive(false);
    }

    private void RememberLoginInfo()
    {
        PlayerPrefs.SetString("email_reme_login", emailInputField.text);
        PlayerPrefs.SetString("date_reme_login", DateTime.Today.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    private void ForgetLoginInfo()
    {
        PlayerPrefs.DeleteKey("email_reme_login");
        PlayerPrefs.DeleteKey("date_reme_login");
        PlayerPrefs.Save();
    }

    private void ClearLoginInfo()
    {
        PlayerPrefs.DeleteKey("session_token");
        PlayerPrefs.Save();
        ForgetLoginInfo();
    }

    private void OpenLoginErrorPanel()
    {
        validLoginPassword.gameObject.SetActive(true);
        passwordInputField.text = "";
    }

    public void VerifyEmail()
    {
        validLoginEmail.gameObject.SetActive(false);

        validEmail = ParseString.IsEmail(emailInputField.text);
        validEmailLabel.gameObject.SetActive(!validEmail);
    }

    public async void OnLogin()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);

        if (!validEmail)
        {
            return;
        }

        bool successfulLogin =
            await AuthenticationManager.Instance.Login(emailInputField.text, passwordInputField.text);
        UpdatePanelOnAuthenticated(successfulLogin);
    }

    public void UpdatePanelOnAuthenticated(bool successfulLogin)
    {
        if (successfulLogin)
        {
            RememberAndCloseLoginPanel();
        }
        else
        {
            Debug.LogWarning("-------------------Login Error------------------");
            OpenLoginErrorPanel();
            ClearLoginInfo();
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