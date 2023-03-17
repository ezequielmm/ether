using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MainMenuTests
{
    private MainMenuManager mainMenu;
    private WalletManager walletManager;
    private UserDataManager userData;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/MainMenu");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        mainMenu = GameObject.FindObjectOfType<MainMenuManager>();
        walletManager = WalletManager.Instance;
        userData = UserDataManager.Instance;
    }
    
    [UnityTearDown]
    public IEnumerator Teardown()
    {
        walletManager.DestroyInstance();
        userData.DestroyInstance();
        GameManager.Instance.DestroyInstance();
        yield return null;
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
        mainMenu.UpdateNameAndFief("test", 0);
        yield return null;
        Assert.AreEqual("test", mainMenu.nameText.text);
        mainMenu.UpdateNameAndFief("No One Can Stop the Ping Pong in the Bayou", 0);
        yield return null;
        Assert.AreEqual("No One Can Stop the Ping Pong in the Bayou", mainMenu.nameText.text);
    }

    [UnityTest]
    public IEnumerator TestFiefIsUpdatedOnSuccessfulLogin()
    {
        mainMenu.UpdateNameAndFief("", 0);
        yield return null;
        Assert.AreEqual("0 $fief", mainMenu.moneyText.text);
        mainMenu.UpdateNameAndFief("", 45342);
        yield return null;
        Assert.AreEqual("45342 $fief", mainMenu.moneyText.text);
    }

    [UnityTest]
    public IEnumerator TestMenuButtonsAreDeactivatedBySuccessfulLogin()
    {
        GameManager.Instance.EVENT_AUTHENTICATED.Invoke();
        yield return null;
        Assert.AreEqual(false, mainMenu.playButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.registerButton.gameObject.activeSelf);
        Assert.AreEqual(false, mainMenu.loginButton.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator TestNameAndFiefUpdateListeners()
    {
        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.Invoke("test", 69420);
        yield return null;

        Assert.AreEqual("test", mainMenu.nameText.text);
        Assert.AreEqual("69420 $fief", mainMenu.moneyText.text);
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
    public IEnumerator DoesPlayButtonTextChangeIfNoExpedition()
    {
        TextMeshProUGUI playButtonText = mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>();
        UserDataManager.Instance.ClearExpedition();
        yield return null;
        Assert.AreEqual("PLAY", playButtonText.text);
        Assert.False(mainMenu.playButton.gameObject.activeSelf);
        Assert.False(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.False(mainMenu.treasuryButton.gameObject.activeSelf);
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
    
    [UnityTest]
    public IEnumerator NoWalletScreen()
    {
        SetHasWallet(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsFalse(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.playButton.interactable);
    }
    
    [UnityTest]
    public IEnumerator WalletButNotVerified()
    {
        SetHasWallet(true);
        SetWalletVerified(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsTrue(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.playButton.interactable);
        Assert.AreEqual("Verifying...", mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>().text);
    }
    
    [UnityTest]
    public IEnumerator WalletVerifiedButWaitingOnExpeditionStatus()
    {
        SetHasWallet(true);
        SetWalletVerified(true);
        SetExpeditionStatusReceived(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsTrue(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.playButton.interactable);
        Assert.AreEqual("Verifying...", mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>().text);
    }
    
    [UnityTest]
    public IEnumerator WalletVerifiedButNoNfts()
    {
        SetHasWallet(true);
        SetWalletVerified(true);
        SetExpeditionStatusReceived(true);
        SetOwnsAnyNft(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsFalse(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.playButton.interactable);
    }
    
    [UnityTest]
    public IEnumerator ValidatedNoExpedition()
    {
        SetHasWallet(true);
        SetWalletVerified(true);
        SetExpeditionStatusReceived(true);
        SetOwnsAnyNft(true);
        SetHasExpedition(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsTrue(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsTrue(mainMenu.playButton.interactable);
        Assert.AreEqual("PLAY", mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>().text);
    }
    
    [UnityTest]
    public IEnumerator ValidatedExpeditionInvalidNft()
    {
        SetHasWallet(true);
        SetWalletVerified(true);
        SetExpeditionStatusReceived(true);
        SetOwnsAnyNft(true);
        SetHasExpedition(true);
        SetOwnsSavedNft(false);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsTrue(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsFalse(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsTrue(mainMenu.playButton.interactable);
        Assert.AreEqual("PLAY", mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>().text);
    }
    
    [UnityTest]
    public IEnumerator ValidatedExpeditionValidNft()
    {
        SetHasWallet(true);
        SetWalletVerified(true);
        SetExpeditionStatusReceived(true);
        SetOwnsAnyNft(true);
        SetHasExpedition(true);
        SetOwnsSavedNft(true);
        mainMenu.VerifyResumeExpedition();
        yield return null;
        Assert.IsTrue(mainMenu.playButton.gameObject.activeSelf);
        Assert.IsTrue(mainMenu.newExpeditionButton.gameObject.activeSelf);
        Assert.IsTrue(mainMenu.playButton.interactable);
        Assert.AreEqual("RESUME", mainMenu.playButton.GetComponentInChildren<TextMeshProUGUI>().text);
    }

    #region HelperSpecificTests
    [Test]
    public void HasWalletFalse()
    {
        SetHasWallet(false);
        Assert.IsFalse(mainMenu._hasWallet);
    }
    [Test]
    public void HasWalletTrue()
    {
        SetHasWallet(true);
        Assert.IsTrue(mainMenu._hasWallet);
    }
    [Test]
    public void WalletVerifiedFalse()
    {
        SetWalletVerified(false);
        Assert.IsFalse(mainMenu._isWalletVerified);
    }
    [Test]
    public void WalletVerifiedTrue()
    {
        SetWalletVerified(true);
        Assert.IsTrue(mainMenu._isWalletVerified);
    }
    [Test]
    public void OwnsAnyNftFalse()
    {
        SetOwnsAnyNft(false);
        Assert.IsFalse(mainMenu._ownsAnyNft);
    }
    [Test]
    public void OwnsAnyNftTrue()
    {
        SetOwnsAnyNft(true);
        Assert.IsTrue(mainMenu._ownsAnyNft);
    }
    [Test]
    public void ExpeditionStatusReceivedFalse()
    {
        SetExpeditionStatusReceived(false);
        Assert.IsFalse(mainMenu._expeditionStatusReceived);
    }
    [Test]
    public void ExpeditionStatusReceivedTrue()
    {
        SetExpeditionStatusReceived(true);
        Assert.IsTrue(mainMenu._expeditionStatusReceived);
    }
    [Test]
    public void HasExpeditionFalse()
    {
        SetHasExpedition(false);
        Assert.IsFalse(mainMenu._hasExpedition);
    }
    [Test]
    public void HasExpeditionTrue()
    {
        SetHasExpedition(true);
        Assert.IsTrue(mainMenu._hasExpedition);
    }
    [Test]
    public void OwnsSavedNftFalse()
    {
        SetOwnsSavedNft(false);
        Assert.IsFalse(mainMenu._ownsSavedNft);
    }
    [Test]
    public void OwnsSavedNftTrue()
    {
        SetOwnsSavedNft(true);
        Assert.IsTrue(mainMenu._ownsSavedNft);
    }
    #endregion
    #region MainMenuProgressionBoolHelpers
    private void SetHasWallet(bool value)
    {
        walletManager.ActiveWallet = value ? "0xFAKEWALLET" : null;
    }
    private void SetWalletVerified(bool value)
    {
        walletManager.WalletVerified = value;
    }
    private void SetOwnsAnyNft(bool value)
    {
        if (!value)
        {
            walletManager.NftsInWallet.Clear();
            return;
        }
        if(!walletManager.NftsInWallet.ContainsKey(NftContract.None))
            walletManager.NftsInWallet.Add(NftContract.None, new List<int>() { -1 });
    }
    private void SetExpeditionStatusReceived(bool value)
    {
        mainMenu._expeditionStatusReceived = value;
    }
    private void SetHasExpedition(bool value)
    {
        userData.SetExpedition(new ExpeditionStatus(){ HasExpedition = value});
    }
    private void SetOwnsSavedNft(bool value)
    {
        userData.SetExpedition(new ExpeditionStatus(){ HasExpedition = value, NftId = -1});
        if(value && !walletManager.NftsInWallet.ContainsKey(NftContract.None))
            walletManager.NftsInWallet.Add(NftContract.None, new List<int>() { -1 });
    }
    #endregion
}