
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText, koteLabel;
    [Tooltip("the entire button panel's canvas group for controling them all")]
    public CanvasGroup buttonPanel;
    [Tooltip("Main menu buttons for individual control")]
    public Button playButton, newExpeditionButton, treasuryButton, registerButton, loginButton, nameButton, fiefButton, settingButton;

    private bool _hasWallet;
    private bool _hasExpedition;

    private void Start()
    {
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLoginSuccessful);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionUpdate);

        TogglePreLoginStatus(true);
    }

    private void OnExpeditionUpdate(bool hasExpedition)
    {
        TextMeshProUGUI textField = playButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();

        _hasExpedition = hasExpedition;

        textField?.SetText( hasExpedition? "RESUME" : "PLAY");

        playButton.gameObject.SetActive(true);
        newExpeditionButton.gameObject.SetActive(_hasExpedition);
        treasuryButton.gameObject.SetActive(true);

        
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
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnLoginButton()
    {
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnSettingsButton()
    {
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnTreasuryButton()
    {
        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnPlayButton()
    {
        //check if we are playing a new expedition or resuming
        if (_hasExpedition)
        {
            //load the expedition
            GameManager.Instance.LoadScene(inGameScenes.Expedition);
        }
        else
        {
            // if there's no wallet, ask if they want to connect one
            if (!CheckForWallet())
            {
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(
                    "No Wallet connected, would you like to add one?",
                    () => { GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true); },
                    () => { GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true); },
                    new []{"Manage Wallet", "Play Without Wallet"});
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
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Do you want to cancel the current expedition?", OnNewExpeditionConfirmed);
    }

    public void OnNewExpeditionConfirmed()
    {
        // cancel the expedition
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.Invoke();
              
    }

}