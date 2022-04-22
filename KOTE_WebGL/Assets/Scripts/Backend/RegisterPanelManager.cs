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

    [Space(20)]
    public TMP_Text validEmailLabel;

    public TMP_Text emailNotMatchLabel;
    public TMP_Text validPasswordLabel;
    public TMP_Text passwordNotMatchLabel;
    public TMP_Text nameText;

    [Space(20)]
    public Toggle termsAndConditions;

    public Button registerButton;
    public GameObject registerContainer;

    private bool validEmail;
    private bool emailConfirmed;

    private bool validPassword;
    private bool passwordConfirmed;
    public Toggle showPassword;

    private void Awake()
    {
        GameManager.Instance.EVENT_REQUEST_NAME_SUCESSFUL.AddListener(OnNewRandomName);
        GameManager.Instance.EVENT_REQUEST_NAME_ERROR.AddListener(OnRandomNameError); //TODO: move this to error manager
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSucessful);
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.AddListener(OnLoginError);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerRegisterPanel);
    }

    public void OnShowPassword()
    {
        passwordInputField.contentType = showPassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
        confirmPasswordInputField.contentType = showPassword.isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        confirmPasswordInputField.ForceLabelUpdate();
    }

    private void OnNewRandomName(string newName)
    {
        nameText.SetText(newName);
    }

    private void OnRandomNameError(string errorMessage)
    {
        throw new NotImplementedException();
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

        registerButton.interactable = false;

        GameManager.Instance.EVENT_REQUEST_NAME.Invoke("");
    }

    private void DeactivateAllErrorLabels()
    {
        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);
    }

    public void UpdateRegisterButton()
    {
        registerButton.interactable = emailConfirmed && passwordConfirmed && termsAndConditions.isOn;
    }

    #region
    public bool VerifyEmail()
    {   
        validEmail = ParseString.IsEmail(emailInputField.text);
        validEmailLabel.gameObject.SetActive(!validEmail);
        return validEmail;
    }

    public bool ConfirmEmail()
    {
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
    #endregion

    public void RequestNewName()
    {
        nameText.text = "Loading...";
        GameManager.Instance.EVENT_REQUEST_NAME.Invoke(nameText.text);
    }

    public void OnRegister()
    {
        string name = nameText.text;
        string email = emailInputField.text;
        string password = passwordInputField.text;
        GameManager.Instance.EVENT_REQUEST_REGISTER.Invoke(name, email, password);
    }

    public void LoginHyperlink()
    {
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

    public void PerfomAllValidations()
    {
        DeactivateAllErrorLabels();

        if(!VerifyEmail())return;
        if(!ConfirmEmail())return;
        if(!VerifyPassword())return;
        if(!ConfirmPassword())return;

        UpdateRegisterButton();
    }
}