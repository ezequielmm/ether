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
    public Button registerButton;
    public GameObject registerContainer;


    private bool validEmail;
    private bool emailConfirmed;

    private bool validPassword;
    private bool passwordConfirmed;

    private void Awake()
    {
        GameManager.Instance.EVENT_NEW_RANDOM_NAME.AddListener(OnNewRandomName);
        GameManager.Instance.EVENT_REGISTER_COMPLETED.AddListener(OnRegisterCompleted);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerRegisterPanel);
    }

    private void Start()
    {
        validEmailLabel.gameObject.SetActive(false);
        emailNotMatchLabel.gameObject.SetActive(false);
        validPasswordLabel.gameObject.SetActive(false);
        passwordNotMatchLabel.gameObject.SetActive(false);

        registerButton.interactable = false;

        GameManager.Instance.EVENT_REQUEST_NAME.Invoke(true, null);
    }

    private void Update()
    {
        registerButton.interactable = emailConfirmed && passwordConfirmed && termsAndConditions.isOn;
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
        GameManager.Instance.EVENT_REQUEST_NAME.Invoke(false, nameText.text);
    }

    public void OnNewRandomName(string name)
    {
        nameText.text = name;
    }

    public void OnRegister()
    {
        string name = nameText.text;
        string email = emailInputField.text;
        string password = passwordInputField.text;
        GameManager.Instance.EVENT_REGISTER.Invoke(name, email, password);
    }

    public void OnRegisterCompleted(string token)
    {
        PlayerPrefs.SetString("session_token", token);
        PlayerPrefs.Save();
    }

    public void LoginHyperlink()
    {
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerRegisterPanel(bool activate)
    {
        registerContainer.SetActive(activate);
        Image panelImage = gameObject.GetComponent<Image>();
        panelImage.raycastTarget = activate;
        Color tempColor = panelImage.color;
        tempColor.a = activate ? 1f : 0.004f;
        panelImage.color = tempColor;
    }
}