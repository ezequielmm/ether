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
    public TMP_InputField emailInputField;
    public TMP_InputField confirmEmailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField confirmPasswordInputField;

    [Space(20)] public TMP_Text validEmailLabel;
    public TMP_Text emailNotMatchLabel;
    public TMP_Text validPasswordLabel;
    public TMP_Text passwordNotMatchLabel;
    public TMP_Text nameText;

    [Space(20)] public Toggle termsAndConditions;

    [Space(20)] public LoginPanelManager loginPanelManager;
    public WebRequesterManager webRequesterManager;

    private bool validEmail;
    private bool emailConfirmed;

    private bool validPassword;
    private bool passwordConfirmed;

    private void Start()
    {
        webRequesterManager.RequestNewName(nameText);

        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    public void VerifyEmail()
    {
        validEmail = ParseString.IsEmail(emailInputField.text);
        validEmailLabel.gameObject.SetActive(!validEmail);

        emailNotMatchLabel.gameObject.SetActive(false);
        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    public void ConfirmEmail()
    {
        emailConfirmed = validEmail && (emailInputField.text == confirmEmailInputField.text);
        emailNotMatchLabel.gameObject.SetActive(!emailConfirmed && validEmail);

        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    public void VerifyPassword()
    {
        validPassword = ParseString.IsPassword(passwordInputField.text);
        validPasswordLabel.gameObject.SetActive(!validPassword);

        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    public void ConfirmPassword()
    {
        passwordConfirmed = validPassword && (passwordInputField.text == confirmPasswordInputField.text);
        passwordNotMatchLabel.gameObject.SetActive(!passwordConfirmed && validPassword);

        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
    }

    public void RequestNewName()
    {
        webRequesterManager.RequestNewName(nameText);
    }

    public void OnRegister()
    {
        if (passwordConfirmed && emailConfirmed && termsAndConditions.isOn)
        {
            string email = emailInputField.text;
            string password = passwordInputField.text;
            webRequesterManager.RequestRegister(nameText.text, email, password);
        }
    }

    public void LoginHyperlink()
    {
        gameObject.SetActive(false);
        loginPanelManager.gameObject.SetActive(true);
    }
}