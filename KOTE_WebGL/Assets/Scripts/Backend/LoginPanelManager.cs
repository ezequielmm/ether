using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Toggle = UnityEngine.UI.Toggle;

public class LoginPanelManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    [Space(20)] public TMP_Text validEmailLabel;
    public TMP_Text validLoginEmail;
    public TMP_Text validLoginPassword;

    [Space(20)] public Toggle rememberMe;
    public WebRequester webRequester;

    private bool validEmail;
    private bool validLogin;

    private void Start()
    {
        validEmailLabel.gameObject.SetActive(false);
        validLoginEmail.gameObject.SetActive(false);
        validLoginPassword.gameObject.SetActive(false);
    }

    public void VerifyEmail()
    {
        validEmail = ParseString.IsEmail(emailInputField.text);
        Debug.Log($"valid email");
        validLoginEmail.gameObject.SetActive(validEmail && validLogin);
        validEmailLabel.gameObject.SetActive(!validEmail);
    }

    public void OnLogin()
    {
        validLoginEmail.gameObject.SetActive(false);
        validLoginEmail.gameObject.SetActive(false);


        if (validEmail)
        {
            validLogin = webRequester.RequestLogin(emailInputField.text, passwordInputField.text);

            if (validLogin)
            {
            }
            else
            {
                validLoginEmail.gameObject.SetActive(!validLogin);
                validLoginPassword.gameObject.SetActive(!validLogin);
            }
        }
    }
}