using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class RegisterPanelManagerTests : MonoBehaviour
{
    private RegisterPanelManager registerPanel;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject registerPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/RegisterPanel.prefab");
        GameObject registerManager = Instantiate(registerPanelPrefab);
        registerPanel = registerManager.GetComponent<RegisterPanelManager>();
        registerManager.SetActive(true);
        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(registerPanel.gameObject);
        yield return null;
    }

    [Test]
    public void DoesOnShowPasswordChangeInputFieldContentType()
    {
        Assert.AreEqual(TMP_InputField.ContentType.Password, registerPanel.passwordInputField.contentType);
        registerPanel.showPassword.isOn = true;
        Assert.AreEqual(TMP_InputField.ContentType.Standard, registerPanel.passwordInputField.contentType);
        registerPanel.showPassword.isOn = false;
        Assert.AreEqual(TMP_InputField.ContentType.Password, registerPanel.passwordInputField.contentType);
    }

    [Test]
    public void DoesOnShowConfirmPasswordChangeInputFieldContentType()
    {
        Assert.AreEqual(TMP_InputField.ContentType.Password, registerPanel.confirmPasswordInputField.contentType);
        registerPanel.showConfirmPassword.isOn = true;
        Assert.AreEqual(TMP_InputField.ContentType.Standard, registerPanel.confirmPasswordInputField.contentType);
        registerPanel.showConfirmPassword.isOn = false;
        Assert.AreEqual(TMP_InputField.ContentType.Password, registerPanel.confirmPasswordInputField.contentType);
    }

    [Test]
    public void DoesRequestNameUpdateNameText()
    {
        GameManager.Instance.EVENT_REQUEST_NAME_SUCESSFUL.Invoke("test");
        Assert.AreEqual("test", registerPanel.nameInputField.text);
        GameManager.Instance.EVENT_REQUEST_NAME_SUCESSFUL.Invoke("No One Can Stop The Ping Pong In The Bayou");
        Assert.AreEqual("No One Can Stop The Ping Pong In The Bayou", registerPanel.nameInputField.text);
        GameManager.Instance.EVENT_REQUEST_NAME_SUCESSFUL.Invoke("yup, this be working");
        Assert.AreEqual("yup, this be working", registerPanel.nameInputField.text);
    }

    [Test]
    public void DoesSuccessfulLoginDeactivateRegisterPanel()
    {
        registerPanel.ActivateInnerRegisterPanel(true);
        Assert.True(registerPanel.registerContainer.activeSelf);
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke("", 0);
        Assert.False(registerPanel.registerContainer.activeSelf);
    }

    [Test]
    public void DoesLoginErrorLogTheError()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke("test");
        LogAssert.Expect(LogType.Log, "Register Error:test");
    }

    [Test]
    public void DoesStartSetRegisterButtonInteractableToFalse()
    {
        Assert.False(registerPanel.registerButton.interactable);
    }

    [UnityTest]
    public IEnumerator DoesStartFireRequestNameEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_REQUEST_NAME.AddListener((data) => { eventFired = true; });
        GameObject registerPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/RegisterPanel.prefab");
        GameObject registerManager = Instantiate(registerPanelPrefab);
        registerPanel = registerManager.GetComponent<RegisterPanelManager>();
        registerManager.SetActive(true);
        yield return null;
        Assert.True(eventFired);
    }

    [Test]
    public void DoesStartDeactivateAllErrorLabels()
    {
        Assert.False(registerPanel.validEmailLabel.gameObject.activeSelf);
        Assert.False(registerPanel.emailNotMatchLabel.gameObject.activeSelf);
        Assert.False(registerPanel.validPasswordLabel.gameObject.activeSelf);
        Assert.False(registerPanel.passwordNotMatchLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesVerifyEmailCheckEmail()
    {
        registerPanel.emailInputField.text = "test";
        Assert.False(registerPanel.VerifyEmail());
        registerPanel.emailInputField.text = "test@gmail.com";
        Assert.True(registerPanel.VerifyEmail());
    }

    [Test]
    public void DoesVerifyEmailActivatedInvalidEmailWarning()
    {
        registerPanel.emailInputField.text = "test";
        registerPanel.VerifyEmail();
        Assert.True(registerPanel.validEmailLabel.gameObject.activeSelf);
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.VerifyEmail();
        Assert.False(registerPanel.validEmailLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesConfirmEmailConfirmThatEmailsMatch()
    {
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.VerifyEmail();
        Assert.True(registerPanel.ConfirmEmail());
        registerPanel.emailInputField.text = "test2@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.VerifyEmail();
        Assert.False(registerPanel.ConfirmEmail());
    }

    [Test]
    public void DoesConfirmEmailFailIfNotValidEmail()
    {
        registerPanel.emailInputField.text = "testgmail.com";
        registerPanel.confirmEmailInputField.text = "testgmail.com";
        registerPanel.VerifyEmail();
        Assert.False(registerPanel.ConfirmEmail());
    }

    [Test]
    public void DoesConfirmEmailShowMismatchedEmailWarning()
    {
        registerPanel.emailInputField.text = "test2@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.VerifyEmail();
        registerPanel.ConfirmEmail();
        Assert.True(registerPanel.emailNotMatchLabel.gameObject.activeSelf);
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.VerifyEmail();
        registerPanel.ConfirmEmail();
        Assert.False(registerPanel.emailNotMatchLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesVerifyPasswordCheckPassword()
    {
        registerPanel.passwordInputField.text = "Abc123!!";
        Assert.True(registerPanel.VerifyPassword());
        registerPanel.passwordInputField.text = ";";
        Assert.False(registerPanel.VerifyPassword());
    }

    [Test]
    public void DoesVerifyPasswordShowInvalidPasswordWarning()
    {
        registerPanel.passwordInputField.text = "hi";
        registerPanel.VerifyPassword();
        Assert.True(registerPanel.validPasswordLabel.gameObject.activeSelf);
        registerPanel.passwordInputField.text = "Testing1@3";
        registerPanel.VerifyPassword();
        Assert.False(registerPanel.validPasswordLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesConfirmPasswordCheckThatPasswordsAreMatching()
    {
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        Assert.True(registerPanel.ConfirmPassword());
        registerPanel.passwordInputField.text = "P@ssw1rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        Assert.False(registerPanel.ConfirmPassword());
    }

    [Test]
    public void DoesConfirmPasswordCheckThatPasswordsAreValid()
    {
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        Assert.True(registerPanel.ConfirmPassword());
        registerPanel.passwordInputField.text = "Psswrd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        Assert.False(registerPanel.ConfirmPassword());
    }

    [Test]
    public void DoesConfirmPasswordShowMismatchedPasswordWarning()
    {
        registerPanel.passwordInputField.text = "P@ssw1rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        registerPanel.ConfirmPassword();
        Assert.True(registerPanel.passwordNotMatchLabel.gameObject.activeSelf);
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.VerifyPassword();
        registerPanel.ConfirmPassword();
        Assert.False(registerPanel.passwordNotMatchLabel.gameObject.activeSelf);
    }

    [Test]
    public void DoesClickingRegisterFireRequestRegisterEvent()
    {
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        bool eventFired = false;
        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener((data1, data2, data3) => { eventFired = true; });
        registerPanel.OnRegister();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingRegisterSendCorrectInformation()
    {
        registerPanel.nameInputField.text = "Gertrude";
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        bool correctName = false;
        bool correctEmail = false;
        bool correctPassword = false;
        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener((sentName, sentEmail, sentPassword) =>
        {
            correctName = (sentName == "Gertrude");
            correctEmail = (sentEmail == "test@gmail.com");
            correctPassword = (sentPassword == "P@ssw0rd");
        });
        registerPanel.OnRegister();
        Assert.True(correctName);
        Assert.True(correctEmail);
        Assert.True(correctPassword);
    }

    [Test]
    public void DoesLoginHyperlinkHideRegisterPanel()
    {
        registerPanel.ActivateInnerRegisterPanel(true);
        Assert.True(registerPanel.registerContainer.activeSelf);
        registerPanel.LoginHyperlink();
        Assert.False(registerPanel.registerContainer.activeSelf);
    }

    [Test]
    public void DoesLoginHyperlinkFireLoginPanelActivationRequest()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener((data) => { eventFired = true; });
        registerPanel.LoginHyperlink();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesActivatingRegisterPanelClearFields()
    {
        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.ActivateInnerRegisterPanel(true);
        Assert.AreEqual("", registerPanel.emailInputField.text);
        Assert.AreEqual("", registerPanel.confirmEmailInputField.text);
        Assert.AreEqual("", registerPanel.passwordInputField.text);
        Assert.AreEqual("", registerPanel.confirmPasswordInputField.text);

        registerPanel.emailInputField.text = "test@gmail.com";
        registerPanel.confirmEmailInputField.text = "test@gmail.com";
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.confirmPasswordInputField.text = "P@ssw0rd";
        registerPanel.ActivateInnerRegisterPanel(false);
        Assert.AreEqual("", registerPanel.emailInputField.text);
        Assert.AreEqual("", registerPanel.confirmEmailInputField.text);
        Assert.AreEqual("", registerPanel.passwordInputField.text);
        Assert.AreEqual("", registerPanel.confirmPasswordInputField.text);
    }

    [Test]
    public void DoesActivatingRegisterPanelTogglePanelContainer()
    {
        registerPanel.ActivateInnerRegisterPanel(true);
        Assert.True(registerPanel.registerContainer.activeSelf);
        registerPanel.ActivateInnerRegisterPanel(false);
        Assert.False(registerPanel.registerContainer.activeSelf);
    }

    [Test]
    public void DoesEmailToShortDeactivateRegisterButton()
    {
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.termsAndConditions.isOn = true;
        registerPanel.registerButton.interactable = true;
        
        registerPanel.emailInputField.text = "test";
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.False(registerPanel.registerButton.interactable);
        
        registerPanel.emailInputField.text = "longeremailtest";
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.True(registerPanel.registerButton.interactable);
    }

    [Test]
    public void DoesPasswordToShortDeactivateRegisterButton()
    {
        registerPanel.emailInputField.text = "longeremailtest";
        registerPanel.termsAndConditions.isOn = true;
        registerPanel.registerButton.interactable = true;
        
        registerPanel.passwordInputField.text = "test";
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.False(registerPanel.registerButton.interactable);
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.True(registerPanel.registerButton.interactable);
    }

    [Test]
    public void DoesTermsAndConditionsToggleDeactivateRegisterButton()
    {
        registerPanel.emailInputField.text = "longeremailtest";
        registerPanel.passwordInputField.text = "P@ssw0rd";
        registerPanel.registerButton.interactable = true;
        
        registerPanel.termsAndConditions.isOn = false;
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.False(registerPanel.registerButton.interactable);
        registerPanel.termsAndConditions.isOn = true;
        registerPanel.CheckIfCanActivateRegisterButton();
        Assert.True(registerPanel.registerButton.interactable);
    }
}