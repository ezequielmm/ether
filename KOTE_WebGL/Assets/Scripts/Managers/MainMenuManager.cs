using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText, koteLabel;

    [Tooltip("the entire button panel's canvas group for controlling them all")]
    public CanvasGroup buttonPanel;

    [Tooltip("Main menu buttons for individual control")]
    public Button playButton,
        newExpeditionButton,
        treasuryButton,
        registerButton,
        loginButton,
        nameButton,
        fiefButton,
        connectWalletButton;

    private WalletManager wallet => WalletManager.Instance;
    private UserDataManager userData => UserDataManager.Instance;

    public bool _hasWallet => !string.IsNullOrEmpty(wallet.ActiveWallet);

    // we need to confirm all verification values before showing the play button
    public bool _hasExpedition => userData.HasExpedition;
    [HideInInspector]
    public bool _expeditionStatusReceived;

    // verification that the player still owns the continuing nft
    [HideInInspector]
    public bool _ownsSavedNft => wallet.ConfirmNftOwnership(userData.ActiveNft, userData.NftContract);
    
    // verification that the connected wallet contains at least one knight
    public bool _ownsAnyNft => wallet.ConfirmOwnsNfts();
    public bool _isWalletVerified => wallet.WalletVerified;
    public bool _isWhitelisted => true;

    private void Start()
    {
        AuthenticationManager.Instance.SetSessionToken(null);
        GameManager.Instance.EVENT_UPDATE_NAME_AND_FIEF.AddListener(UpdateNameAndFief);
        GameManager.Instance.EVENT_AUTHENTICATED.AddListener(SetupPostAuthenticationButtons);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        wallet.WalletStatusModified.AddListener(UpdateUiOnWalletModification);
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

    public async void VerifyResumeExpedition()
    {
        if (!_hasWallet || !_isWhitelisted ) 
        {
            playButton.gameObject.SetActive(false);
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = false;
            return;
        }

        
        if (!_isWalletVerified || !_expeditionStatusReceived)
        {
            playButton.gameObject.SetActive(true);
            UpdatePlayButtonText("Verifying...");
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = false;
            return;
        }
        
        if (!_ownsAnyNft ) 
        {
            playButton.gameObject.SetActive(false);
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = false;
            return;
        }
        
        if (!_hasExpedition)
        {
            UpdatePlayButtonTextForExpedition();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(false);
            playButton.interactable = true;
        }
        else if (_ownsSavedNft)
        {
            UpdatePlayButtonTextForExpedition();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(true);
            playButton.interactable = true;
        }
        // if the player no longer owns the nft, clear the expedition
        else
        {
            await userData.ClearExpedition();
            VerifyResumeExpedition();
        }
    }

    private void UpdateUiOnWalletModification() 
    {
        connectWalletButton.gameObject.SetActive(!_hasWallet);
        VerifyResumeExpedition();
    }


    private void UpdatePlayButtonText(string value)
    {
        TextMeshProUGUI textField = playButton.GetComponentInChildren<TextMeshProUGUI>();
        textField?.SetText(value);
    }

    private void UpdatePlayButtonTextForExpedition() 
    {
        UpdatePlayButtonText(_hasExpedition ? "RESUME" : "PLAY");
    }

    public void UpdateNameAndFief(string name, int fief)
    {
        nameText.text = name;
        moneyText.text = $"{fief} $fief";
    }

    public void OnLogoutSuccessful(string message)
    {
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
        connectWalletButton.gameObject.SetActive(!preLoginStatus);
    }

    private void SetupPostAuthenticationButtons()
    {
        playButton.gameObject.SetActive(false);
        newExpeditionButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
        connectWalletButton.gameObject.SetActive(!_hasWallet);
        GetExpeditionStatus();
    }

    private async void GetExpeditionStatus() 
    {
        await userData.UpdateExpeditionStatus();

        _expeditionStatusReceived = true;
        PlayerSpriteManager.Instance.SetSkin(userData.ActiveNft, userData.NftContract);

        VerifyResumeExpedition();
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

    public void OnTreasuryButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnWalletConnectButton()
    {
        wallet.SetActiveWallet();
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
                    () => { wallet.SetActiveWallet(); },
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

    public async void OnNewExpeditionConfirmed()
    {
        _expeditionStatusReceived = false;
        await userData.ClearExpedition();
        GetExpeditionStatus();
    }
}