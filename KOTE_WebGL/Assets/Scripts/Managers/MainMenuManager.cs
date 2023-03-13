using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText, koteLabel;

    [Tooltip("the entire button panel's canvas group for controling them all")]
    public CanvasGroup buttonPanel;

    [Tooltip("Main menu buttons for individual control")]
    public Button playButton,
        newExpeditionButton,
        treasuryButton,
        registerButton,
        loginButton,
        nameButton,
        fiefButton,
        settingButton,
        connectWalletButton;

    [SerializeField]
    private GameObject ContestTimer;

    private bool _hasWallet => !string.IsNullOrEmpty(WalletManager.Instance.ActiveWallet);

    // we need to confirm all verification values before showing the play button
    private bool _hasExpedition;
    private bool _expeditionStatusReceived;

    // verification that the player still owns the continuing nft
    private bool _ownsSavedNft => WalletManager.Instance.NftsInWallet?
        .GetValueOrDefault(NftContract.Knights)?.Contains(_nftInExpedition) ?? false;
    private int _nftInExpedition = -1;
    
    // verification that the connected wallet contains at least one knight
    private bool _ownsAnyNft => (WalletManager.Instance.NftsInWallet?.GetValueOrDefault(NftContract.Knights)?.Count ?? 0) > 0;
    private bool _isWalletVerified => WalletManager.Instance.WalletVerified;
    private bool _isWhitelisted => true;

    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSuccessful);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionUpdate);

        WalletManager.Instance.WalletStatusModified.AddListener(UpdateUiOnWalletModification);
        NftManager.Instance.NftsLoaded.AddListener(VerifyResumeExpedition);

        //CheckIfRegisterButtonIsEnabled();
        CheckIfArmoryButtonIsEnabled();

        TogglePreLoginStatus(true);
        
        // default the play button to not being interactable
        playButton.interactable = false;
        
    }

    private void CheckIfArmoryButtonIsEnabled()
    {
        int enableRegistration = PlayerPrefs.GetInt("enable_armory");
        if (enableRegistration == 1)
        {
            treasuryButton.interactable = true;
        }
        else
        {
            treasuryButton.interactable = false;
        }
    }

    // callbacks for verifying the player can play the game
    private void OnExpeditionUpdate(ExpeditionStatusData data)
    {
        _nftInExpedition = data.nftId;
        _expeditionStatusReceived = true;
        _hasExpedition = data.hasExpedition;
        treasuryButton.gameObject.SetActive(true);

        VerifyResumeExpedition();
    }

    // we need to verify that the player can actually resume or start a new game before presenting those options
    // this is designed to be called whenever a callback is triggered, due to not knowing when all the responses will come in
    private void VerifyResumeExpedition()
    {
        
        // if the player isn't whitelisted, doesn't have a wallet connected, or doesn't own a knight, never show the play button
        if (!_hasWallet || !_isWalletVerified || !_isWhitelisted || !_ownsAnyNft)
        {
            // if no routes are available, lock the player out of the game
            playButton.gameObject.SetActive(false);
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = false;
            return;
        }
        
        if (_expeditionStatusReceived && !_hasExpedition)
        {
            UpdatePlayButtonText();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = true;
        }

        if (_hasExpedition && _ownsSavedNft)
        {
            UpdatePlayButtonText();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(true);
            playButton.interactable = true;
        }
        // if the player no longer owns the nft, clear the expedition
        else if (_hasExpedition && !_ownsSavedNft)
        {
            _hasExpedition = false;
            GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.Invoke();
        }
    }

    private void UpdateUiOnWalletModification() 
    {
        connectWalletButton.gameObject.SetActive(!_hasWallet);
        VerifyResumeExpedition();
    }


    private void UpdatePlayButtonText()
    {
        TextMeshProUGUI textField = playButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        textField?.SetText(_hasExpedition ? "RESUME" : "PLAY");
    }

    public void OnLoginSuccessful(string name, int fief)
    {
        nameText.text = name;
        moneyText.text = $"{fief} $fief";
        DeactivateMenuButtons();
        settingButton.gameObject.SetActive(true);
        ContestTimer.SetActive(true);
    }

    public void OnLogoutSuccessful(string message)
    {
        //koteLabel.gameObject.SetActive(true);
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(false);
        TogglePreLoginStatus(true);
    }

    public void TogglePreLoginStatus(bool preLoginStatus)
    {
        nameText.gameObject.SetActive(!preLoginStatus);
        moneyText.gameObject.SetActive(!preLoginStatus);
        playButton.gameObject.SetActive(!preLoginStatus);
        treasuryButton.gameObject.SetActive(!preLoginStatus);
        newExpeditionButton.gameObject.SetActive(!preLoginStatus);
        registerButton.gameObject.SetActive(preLoginStatus);
        loginButton.gameObject.SetActive(preLoginStatus);
        nameButton.gameObject.SetActive(!preLoginStatus);
        fiefButton.gameObject.SetActive(!preLoginStatus);
        settingButton.gameObject.SetActive(!preLoginStatus);
        connectWalletButton.gameObject.SetActive(!preLoginStatus);
        ContestTimer.SetActive(!preLoginStatus);
    }

    private void DeactivateMenuButtons()
    {
        playButton.gameObject.SetActive(false);
        newExpeditionButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
        connectWalletButton.gameObject.SetActive(!_hasWallet);
    }

    public void OnRegisterButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnLoginButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnSettingsButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnTreasuryButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnWalletConnectButton()
    {
        WalletManager.Instance.SetActiveWallet();
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Please check MetaMask to finish connecting your wallet",
            () => { });
    }

    public void OnPlayButton()
    {
        Debug.Log($"Has expedition: {_hasExpedition} is Wallet Verified: {_isWalletVerified} Owns nft {_ownsSavedNft} Ownership Confirmed {false}");
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        //check if we are playing a new expedition or resuming
        if (_hasExpedition)
        {
            //load the expedition
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            GameManager.Instance.LoadScene(inGameScenes.Expedition);
        }
        else
        {
            // if there's no wallet, ask if they want to connect one
            if (!_hasWallet)
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(
                    "No Wallet connected, would you like to add one?",
                    () => { WalletManager.Instance.SetActiveWallet(); }, //TODO:this button was disabled for the client Demo Sept 3 2022
                    () => { GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true); },
                    new[] { "Manage Wallet", "Play Without Wallet" });
                return;
            }

            // else open the armory panel
            //GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true);
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
        }
    }

    public void OnNewExpeditionButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Do you want to cancel the current expedition?",
            OnNewExpeditionConfirmed);
    }

    public void OnNewExpeditionConfirmed()
    {
        // cancel the expedition
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.Invoke();
    }
}