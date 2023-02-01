using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;
using UnityEngine.EventSystems;

public class RegisterPanelManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField confirmEmailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField confirmPasswordInputField;

    [Space(20)]
    public TMP_Text invalidNameVariable;
    public TMP_Text validEmailLabel;
    public TMP_Text emailNotMatchLabel;
    public TMP_Text validPasswordLabel;
    public TMP_Text passwordNotMatchLabel;

    [Space(20)] 
    public Toggle termsAndConditions;

    public Button registerButton;
    public GameObject registerContainer;

    private bool validEmail;
    private bool emailConfirmed;

    private bool validUsername;

    private bool validPassword;
    private bool passwordConfirmed;
    public Toggle showPassword;
    public Toggle showConfirmPassword;

    private void Awake()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSucessful);
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.AddListener(OnLoginError);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerRegisterPanel);
    }

    private void CheckIfRegisterButtonIsEnabled()
    {
        int enableRegistration = PlayerPrefs.GetInt("enable_registration");
        if (enableRegistration == 1)
        {
            registerButton.interactable = true;
        }
        else
        {
            registerButton.interactable = false;
        }
    }

    public void OnShowPassword()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        passwordInputField.contentType =
            showPassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
    }

    public void OnShowConfirmPassword()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        confirmPasswordInputField.contentType = showConfirmPassword.isOn
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        confirmPasswordInputField.ForceLabelUpdate();
    }

    private void OnLoginSucessful(string userName, int fief)
    {
        ActivateInnerRegisterPanel(false);
    }

    private void OnLoginError(string errorMessage)
    {
        Debug.Log("Register Error:" + errorMessage);
    }

    private void Start()
    {
        DeactivateAllErrorLabels();
        GameManager.Instance.EVENT_REQUEST_NAME.Invoke("");
    }

    private void DeactivateAllErrorLabels()
    {
        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    #region

    public bool VerifyEmail()
    {
        emailInputField.text = emailInputField.text.Trim();
        validEmail = ParseString.IsEmail(emailInputField.text);
        validEmailLabel.gameObject.SetActive(!validEmail);
        return validEmail;
    }

    public bool ConfirmEmail()
    {
        confirmEmailInputField.text = confirmEmailInputField.text.Trim();
        emailConfirmed = validEmail && (emailInputField.text == confirmEmailInputField.text);
        emailNotMatchLabel.gameObject.SetActive(!emailConfirmed && validEmail);
        return emailConfirmed;
    }

    public bool VerifyPassword()
    {
        validPassword = ParseString.IsPassword(passwordInputField.text);
        validPasswordLabel.gameObject.SetActive(!validPassword);
        return validPassword;
    }

    public bool ConfirmPassword()
    {
        passwordConfirmed = validPassword && (passwordInputField.text == confirmPasswordInputField.text);
        passwordNotMatchLabel.gameObject.SetActive(!passwordConfirmed && validPassword);

        return passwordConfirmed;
    }

    public bool VerifyUsername()
    {
        nameInputField.text = nameInputField.text.Trim();
        validUsername = true;
        // is long enough && short enough
        if(nameInputField.text.Length < 2 || nameInputField.text.Length > 20) 
        {
            validUsername = false;
            invalidNameVariable.text = "Username must be between 2 and 20 characters long.";
        }
        if (nameInputField.text.Length == 0) 
        {
            validUsername = false;
            invalidNameVariable.text = "Username can not be blank.";
        }
        invalidNameVariable.gameObject.SetActive(!validUsername);
        return validUsername;
    }

    public void VerifyValue(int field) 
    {
        switch (field)
        {
            case 0: // username
                VerifyUsername();
                break;
            case 1:
                VerifyEmail();
                break;
            case 2:
                ConfirmEmail();
                break;
            case 3:
                VerifyPassword();
                break;
            case 4:
                ConfirmPassword();
                break;
        }
    }

    #endregion

    public void OnRegister()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        if (!VerifyUsername()) return;
        if (!VerifyEmail()) return;
        if (!ConfirmEmail()) return;
        if (!VerifyPassword()) return;
        if (!ConfirmPassword()) return;

        string name = nameInputField.text;
        string email = emailInputField.text;
        string password = passwordInputField.text;
        GameManager.Instance.EVENT_REQUEST_REGISTER.Invoke(name, email, password);
    }

    public void LoginHyperlink()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ActivateInnerRegisterPanel(false);
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerRegisterPanel(bool activate)
    {
        emailInputField.text = "";
        confirmEmailInputField.text = "";
        passwordInputField.text = "";
        confirmPasswordInputField.text = "";
        registerContainer.SetActive(activate);
    }

    public void CheckIfCanActivateRegisterButton()
    {
        if (!VerifyUsername() || emailInputField.text.Length < 8 || passwordInputField.text.Length < 8 || !termsAndConditions.isOn)
        {
            registerButton.interactable = false;
            return;
        }

        registerButton.interactable = true;
    }
}