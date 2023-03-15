using System;
using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MainMenuTests
{
    private MainMenuManager mainMenu;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/MainMenu");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        mainMenu = GameObject.Find("MainMenu").GetComponent<MainMenuManager>();
    }

    [UnityTest]
    public IEnumerator MainMenuPrefabExists()
    {
        yield return new WaitForSeconds(0.1f);
        Assert.IsNotNull(GameObject.Find("MainMenu"));
    }

    [UnityTest]
    public IEnumerator MainMenuManagerScriptExists()
    {
        yield return new WaitForSeconds(0.1f);
        Assert.IsNotNull(GameObject.Find("MainMenu").GetComponent<MainMenuManager>());
    }

    [UnityTest]
    public IEnumerator UsedUnityEventsExist()
    {
        yield return null;
        Assert.IsNotNull(GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL);
        Assert.IsNotNull(GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST);
        Assert.IsNotNull(GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST);
    }

    [UnityTest]
    public IEnumerator MainMenuSerializedFieldsArePopulated()
    {
        yield return new WaitForSeconds(0.1f);
        Assert.IsNotNull(mainMenu.nameText);
        Assert.IsNotNull(mainMenu.moneyText);
        Assert.IsNotNull(mainMenu.buttonPanel);
        Assert.IsNotNull(mainMenu.playButton);
        Assert.IsNotNull(mainMenu.newExpeditionButton);
        Assert.IsNotNull(mainMenu.treasuryButton);
        Assert.IsNotNull(mainMenu.registerButton);
        Assert.IsNotNull(mainMenu.loginButton);
        Assert.IsNotNull(mainMenu.nameButton);
        Assert.IsNotNull(mainMenu.fiefButton);
        Assert.IsNotNull(mainMenu.settingButton);
    }

    [UnityTest]
    public IEnumerator PreLoginStatusProperlyToggled()
    {
        mainMenu.TogglePreLoginStatus(true);
        yield return null;
        Assert.AreEqual(false, mainMenu.nameText.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.moneyText.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.treasuryButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.loginButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.nameButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.fiefButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.settingButton.gameObject.activeSelf);
        mainMenu.TogglePreLoginStatus(false);
        yield return null;
        Assert.AreEqual(true, mainMenu.nameText.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.moneyText.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.treasuryButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.loginButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.nameButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.fiefButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.settingButton.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator TestNameIsUpdatedOnSuccessfulLogin()
    {
        mainMenu.OnLoginSuccessful("test", 0);
        yield return null;
        Assert.AreEqual("test", mainMenu.nameText.text);
        mainMenu.OnLoginSuccessful("No One Can Stop the Ping Pong in the Bayou", 0);
        yield return null;
        Assert.AreEqual("No One Can Stop the Ping Pong in the Bayou", mainMenu.nameText.text);
    }

    [UnityTest]
    public IEnumerator TestFiefIsUpdatedOnSuccessfulLogin()
    {
        mainMenu.OnLoginSuccessful("", 0);
        yield return null;
        Assert.AreEqual("0 $fief", mainMenu.moneyText.text);
        mainMenu.OnLoginSuccessful("", 45342);
        yield return null;
        Assert.AreEqual("45342 $fief", mainMenu.moneyText.text);
    }

    [UnityTest]
    public IEnumerator TestMenuButtonsAreDeactivatedBySuccessfulLogin()
    {
        mainMenu.OnLoginSuccessful("", 0);
        yield return null;
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.loginButton.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator TestNameAndFiefUpdateLiseners()
    {
        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.Invoke("test", 69420);
        yield return null;

        Assert.AreEqual("test", mainMenu.nameText.text);
        Assert.AreEqual("69420 $fief", mainMenu.moneyText.text);
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.loginButton.gameObject.activeSelf);
    }

    [Test]
    public void TestOnLogoutSuccessfulListener()
    {
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.Invoke("message");
        Assert.AreEqual(false, mainMenu.nameText.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.moneyText.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.treasuryButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.loginButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.nameButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.fiefButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.settingButton.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator TestMainMenuSetup()
    {
        TextMeshProUGUI playButtonText = mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>();
        UserDataManager.Instance.ClearExpedition();
        yield return null;
        Assert.AreEqual("PLAY", playButtonText.text);
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(true, mainMenu.treasuryButton.gameObject.activeSelf);
    }
    
    [UnityTest]
    public IEnumerator DoesOnLogoutSuccessfulFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener((bool show) => { eventFired = true; });
        mainMenu.OnLogoutSuccessful("test");
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnRegisterButtonFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.AddListener((bool show) => { eventFired = true; });
        mainMenu.OnRegisterButton();
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnLoginButtonFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.AddListener((bool show) => { eventFired = true; });
        mainMenu.OnLoginButton();
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnSettingsButtonFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener((bool show) => { eventFired = true; });
        mainMenu.OnSettingsButton();
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnTreasuryButtonFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.AddListener((bool show) => { eventFired = true; });
        mainMenu.OnTreasuryButton();
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnPlayButtonFireEvents()
    {
        bool showConfirmationPanel = false;
        bool showArmoryPanel = false;

        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.AddListener(
            (string test, Action test2, Action test3, string[] test4) => { showConfirmationPanel = true; });
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(
            (bool show) => { showArmoryPanel = true; });

        mainMenu.OnPlayButton();
        yield return null;
        Assert.AreEqual(true, showConfirmationPanel || showArmoryPanel);
    }

    [UnityTest]
    public IEnumerator DoesOnNewExpeditionButtonFireEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.AddListener((string testString, Action test) =>
        {
            eventFired = true;
        });
        mainMenu.OnNewExpeditionButton();
        yield return null;
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnNewExpeditionClearExpedition()
    {
        mainMenu.OnNewExpeditionConfirmed();
        yield return null;
        Assert.AreEqual(false, UserDataManager.Instance.HasExpedition);
    }
}