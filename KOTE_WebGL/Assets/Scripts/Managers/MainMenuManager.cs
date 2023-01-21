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
        settingButton;

    private bool _hasWallet;
    private int _selectedNft = -1;
    private NftMetaData selectedMetadata;

    // we need to confirm all verification values before showing the play button
    private bool _hasExpedition;
    private bool _expeditionStatusReceived;
    private bool _ownershipChecked;

    // verification that the player still owns the continuing nft
    private bool _ownsNft;

    // verification that the player is whitelisted to play/owns an nft
    private bool _isWhitelisted;

    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSuccessful);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionUpdate);
        GameManager.Instance.EVENT_OWNS_CURRENT_EXPEDITION_NFT.AddListener(OnCurrentNftConfirmed);
       
        GameManager.Instance.EVENT_WHITELIST_CHECK_RECEIVED.AddListener(OnWalletWhitelisted);
        
        // Listen for the metadata for the selected NFT so it can be sent on resume
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(OnCurrentNftDataReceived);
        
        CheckIfRegisterButtonIsEnabled();
        CheckIfArmoryButtonIsEnabled();

        TogglePreLoginStatus(true);
        
        // default the play button to not being interactable
        playButton.interactable = false;
        
    }

    private void CheckIfRegisterButtonIsEnabled()
    {
        int enableRegistration = PlayerPrefs.GetInt("enable_registration");
        if (enableRegistration == 1)
        {
            registerButton.interactable = true;
        }
        else
        {
            registerButton.interactable = false;
        }
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
    private void OnExpeditionUpdate(bool hasExpedition, int nftId)
    {
        _expeditionStatusReceived = true;
        _hasExpedition = hasExpedition;
        _selectedNft = nftId;
        treasuryButton.gameObject.SetActive(true);

        VerifyResumeExpedition();
    }

    private void OnCurrentNftConfirmed(bool ownsNft)
    {
        _ownsNft = ownsNft;
        _ownershipChecked = true;
        VerifyResumeExpedition();
    }

    private void OnCurrentNftDataReceived(NftData nftData)
    {
        if (_selectedNft == -1)
        {
            playButton.interactable = true;
            return;
        }

        foreach (NftMetaData metaData in nftData.assets)
        {
            if (int.Parse(metaData.token_id) == _selectedNft)
            {
                selectedMetadata = metaData;
                playButton.interactable = true;
            }
        }
    }

    private void OnWalletWhitelisted(bool isWhitelisted)
    {
        _isWhitelisted = isWhitelisted;
        VerifyResumeExpedition();
    }

    // we need to verify that the player can actually resume or start a new game before presenting those options
    // this is designed to be called whenever a callback is triggered, due to not knowing when all the responses will come in
    private void VerifyResumeExpedition()
    {
        // if the player isn't whitelisted, never show the play button
        if (!_isWhitelisted)
        {
            // if no routes are available, lock the player out of the game
            playButton.gameObject.SetActive(false);
            newExpeditionButton.gameObject.SetActive(false);
            return;
        }
        
        if (_expeditionStatusReceived && !_hasExpedition)
        {
            UpdatePlayButtonText();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(false);
        }

        // if there is an expedition, we need wait to check if the player owns the nft
        if (!_ownershipChecked) return;

        if (_hasExpedition && _ownsNft)
        {
            UpdatePlayButtonText();
            playButton.gameObject.SetActive(true);
            newExpeditionButton.gameObject.SetActive(true);
        }
        // if the player no longer owns the nft, clear the expedition
        else if (_hasExpedition && !_ownsNft)
        {
            _selectedNft = -1;
            _hasExpedition = false;
            GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.Invoke();
        }
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
    }

    private void DeactivateMenuButtons()
    {
        playButton.gameObject.SetActive(false);
        newExpeditionButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);
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

    public void OnPlayButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        //check if we are playing a new expedition or resuming
        if (_hasExpedition)
        {
            //load the expedition
            // play the correct music depending on where the player is
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Music, 1);
            GameManager.Instance.EVENT_PLAY_MUSIC.Invoke(MusicTypes.Ambient, 1);
            GameManager.Instance.EVENT_NFT_SELECTED.Invoke(selectedMetadata);
            GameManager.Instance.LoadScene(inGameScenes.Expedition);
        }
        else
        {
            // if there's no wallet, ask if they want to connect one
            if (!CheckForWallet())
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(
                    "No Wallet connected, would you like to add one?",
                    //() => { GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true); },                    
                    () =>
                    {
                        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true);
                    }, //TODO:this button was disabled for the client Demo Sept 3 2022
                    () => { GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true); },
                    new[] { "Manage Wallet", "Play Without Wallet" });
                return;
            }

            // else open the armory panel
            GameManager.Instance.EVENT_ARMORYPANEL_ACTIVATION_REQUEST.Invoke(true);
        }
    }

    private bool CheckForWallet()
    {
        //TODO add functionality to check if there's a wallet attached to the account

        // this will return false once so that the functionality can be shown off, this is just for mockup purposes
        if (!_hasWallet)
        {
            _hasWallet = true;
            return false;
        }

        return true;
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