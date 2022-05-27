
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText, koteLabel;

    public Button playButton, newExpeditionButton, treasuryButton, registerButton, loginButton, nameButton, fiefButton, settingButton;
    public GameObject confirmationMenu;

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

        textField?.SetText( hasExpedition? "Resume" : "Play");
        
        newExpeditionButton.gameObject.SetActive(_hasExpedition);
    }

    public void OnLoginSuccessful(string name, int fief)
    {
        nameText.text = name;
        moneyText.text = $"{fief} $fief";

        TogglePreLoginStatus(false);
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
        playButton.interactable = !preLoginStatus;
        treasuryButton.interactable = !preLoginStatus;
        newExpeditionButton.gameObject.SetActive(!preLoginStatus);
        registerButton.gameObject.SetActive(preLoginStatus);
        loginButton.gameObject.SetActive(preLoginStatus);
        registerButton.interactable = preLoginStatus;
        loginButton.interactable = preLoginStatus;
        nameButton.gameObject.SetActive(!preLoginStatus);
        fiefButton.gameObject.SetActive(!preLoginStatus);
        settingButton.gameObject.SetActive(!preLoginStatus);
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
            //show the character selection panel
            //koteLabel.gameObject.SetActive(false);
            treasuryButton.interactable = false;
            GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true);
        }
    }

    public void OnNewExpeditionButton()
    {
        confirmationMenu.SetActive(true);
    }

    public void OnNewExpeditionConfirmButton()
    {
        // cancel the exepedition
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.Invoke();
        confirmationMenu.SetActive(false);
        // then start a new one
        treasuryButton.interactable = false;
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnNewExpeditionBackButton()
    {
        confirmationMenu.SetActive(false);
    }
}