using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class LoginPanelManagerTests : MonoBehaviour
{
    private LoginPanelManager loginPanel;
    private GameObject metaMask;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        PlayerPrefs.DeleteAll();
        GameObject metaMaskPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/MetaMask.prefab");
        metaMask = Instantiate(metaMaskPrefab);
        metaMask.SetActive(true);
        yield return null;

        PlayerPrefs.DeleteAll();
        GameObject loginPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/LoginPanel.prefab");
        GameObject loginManager = Instantiate(loginPanelPrefab);
        loginPanel = loginManager.GetComponent<LoginPanelManager>();
        loginManager.SetActive(true);
        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(loginPanel.gameObject);
        Destroy(metaMask);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesLoginManagerLoadSavedEmail()
    {
        PlayerPrefs.SetString("email_reme_login", "andrewtest@gmail.com");
        PlayerPrefs.SetString("date_reme_login", DateTime.Today.ToString(CultureInfo.InvariantCulture));
        GameObject loginPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/LoginPanel.prefab");
        GameObject loginManager = Instantiate(loginPanelPrefab);
        loginPanel = loginManager.GetComponent<LoginPanelManager>();
        loginManager.SetActive(true);
        yield return null;
        Assert.AreEqual("andrewtest@gmail.com", loginPanel.emailInputField.text);
    }

    [UnityTest]
    public IEnumerator DoesLoginManagerHaveEmptyEmailFieldIfNoSavedEmail()
    {
        PlayerPrefs.DeleteAll();
        GameObject loginPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/LoginPanel.prefab");
        GameObject loginManager = Instantiate(loginPanelPrefab);
        loginPanel = loginManager.GetComponent<LoginPanelManager>();
        loginManager.SetActive(true);
        yield return null;
        Assert.AreEqual("", loginPanel.emailInputField.text);
    }

    [UnityTest]
    public IEnumerator DoesLoginManagerNotShowEmailIfTooMuchTimeHasPassed()
    {
        PlayerPrefs.SetString("email_reme_login", "andrewtest@gmail.com");
        DateTime date = new DateTime(2021, 5, 5, 10, 10, 10);
        string testDate = date.ToString(CultureInfo.InvariantCulture);
        string today = DateTime.Today.ToString(CultureInfo.InvariantCulture);
        PlayerPrefs.SetString("date_reme_login", date.ToString(CultureInfo.InvariantCulture));
        GameObject loginPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/LoginPanel.prefab");
        GameObject loginManager = Instantiate(loginPanelPrefab);
        loginPanel = loginManager.GetComponent<LoginPanelManager>();
        loginManager.SetActive(true);
        yield return null;
        Assert.AreEqual("", loginPanel.emailInputField.text);
    }

    [Test]
    public void DoesShowingPasswordToggleInputFieldContentType()
    {
        Assert.AreEqual(TMP_InputField.ContentType.Password, loginPanel.passwordInputField.contentType);
        loginPanel.showPassword.isOn = true;
        Assert.AreEqual(TMP_InputField.ContentType.Standard, loginPanel.passwordInputField.contentType);
        loginPanel.showPassword.isOn = false;
        Assert.AreEqual(TMP_InputField.ContentType.Password, loginPanel.passwordInputField.contentType);
    }

    [Test]
    public void DoesClickingRegisterButtonHideLoginPanel()
    {
        loginPanel.OnRegisterButton();
        Assert.False(loginPanel.loginContainer.activeSelf);
    }

    [Test]
    public void DoesClickingRegisterButtonFireRegisterPanelShowRequest()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.AddListener((arg0 => { eventFired = true; }));
        loginPanel.OnRegisterButton();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesSuccessfulLoginSaveCurrentEmail()
    {
        loginPanel.emailInputField.text = "andrewtest@gmail.com";
        loginPanel.rememberMe.isOn = true;
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke("", 0);
        Assert.AreEqual("andrewtest@gmail.com", PlayerPrefs.GetString("email_reme_login"));
    }

    [Test]
    public void DoesSuccessfulLoginSaveCurrentDate()
    {
        loginPanel.emailInputField.text = "andrewtest@gmail.com";
        loginPanel.rememberMe.isOn = true;
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke("", 0);
        DateTime savedDate = DateTime.ParseExact(PlayerPrefs.GetString("date_reme_login"), "MM/dd/yyyy HH:mm:ss",
            CultureInfo.InvariantCulture);
        DateTime now = DateTime.Today;
        Assert.AreEqual(now.Year, savedDate.Year);
        Assert.AreEqual(now.Month, savedDate.Month);
        Assert.AreEqual(now.Day, savedDate.Day);
        Assert.AreEqual(now.Hour, savedDate.Hour);
        Assert.AreEqual(now.Minute, savedDate.Minute);
        Assert.AreEqual(now.Second, savedDate.Second);
    }

    [Test]
    public void DoesLoginErrorActivateValidLoginPasswordError()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke("");
        Assert.True(loginPanel.validLoginEmail);
    }

    [Test]
    public void DoesLoginErrorClearPasswordInputField()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke("");
        Assert.AreEqual("", loginPanel.passwordInputField.text);
    }

    [Test]
    public void DoesLoginErrorClearSavedEmailAndDate()
    {
        PlayerPrefs.SetString("email_reme_login", "andrewtest@gmail.com");
        PlayerPrefs.SetString("date_reme_login", DateTime.Today.ToString(CultureInfo.InvariantCulture));
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke("");
        Assert.AreEqual(string.Empty, PlayerPrefs.GetString("email_reme_login"));
        Assert.AreEqual(string.Empty, PlayerPrefs.GetString("date_reme_login"));
    }

    [Test]
    public void DoesLoginErrorLogThatThereWasALoginError()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke("");
        LogAssert.Expect(LogType.Log, "-------------------Login Error------------------");
    }

    [Test]
    public void DoesStartHideLoginPanelErrorLabels()
    {
        Assert.False(loginPanel.validEmailLabel.gameObject.activeSelf);
        Assert.False(loginPanel.validLoginEmail.gameObject.activeSelf);
        Assert.False(loginPanel.validLoginPassword.gameObject.activeSelf);
    }

    [Test]
    public void DoesStartDeactivateLoginButton()
    {
        Assert.False(loginPanel.loginButton.interactable);
    }

    [UnityTest]
    public IEnumerator DoesUpdateTurnOnLoginButtonIfThereIsAValidEmailAndAPassword()
    {
        Assert.False(loginPanel.loginButton.interactable);
        loginPanel.emailInputField.text = "andrewtest@gmail.com";
        loginPanel.passwordInputField.text = "test";
        loginPanel.VerifyEmail();
        yield return null;
        Assert.True(loginPanel.loginButton.interactable);
    }

    [Test]
    public void DoesVerifyEmailToggleValidEmailErrorBasedOnCurrentEnteredEmail()
    {
        loginPanel.emailInputField.text = "test";
        loginPanel.VerifyEmail();
        Assert.True(loginPanel.validEmailLabel.gameObject.activeSelf);
        loginPanel.emailInputField.text = "andrewtest@gmail.com";
        loginPanel.VerifyEmail();
        Assert.False(loginPanel.validEmailLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesOnLoginHideValidEmailError()
    {
        loginPanel.validLoginEmail.gameObject.SetActive(true);
        loginPanel.OnLogin();
        Assert.False(loginPanel.validLoginEmail.gameObject.activeSelf);
    }
    
    [Test]
    public void DoesOnLoginHideValidPasswordError()
    {
        loginPanel.validLoginPassword.gameObject.SetActive(true);
        loginPanel.OnLogin();
        Assert.False(loginPanel.validLoginPassword.gameObject.activeSelf);
    }

    [Test]
    public void DoesOnLoginFireRequestLoginEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_REQUEST_LOGIN.AddListener(((arg0, s) => { eventFired = true;}));
        loginPanel.emailInputField.text = "andrewtest@gmail.com";
        loginPanel.VerifyEmail();
        loginPanel.OnLogin();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesSuccessfulLogoutClearPasswordField()
    {
        loginPanel.passwordInputField.text = "test";
        loginPanel.OnLogoutSuccessful("");
        Assert.AreEqual(String.Empty, loginPanel.passwordInputField.text);
    }
    
}