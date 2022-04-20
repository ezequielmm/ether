using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class TopBarManager : MonoBehaviour
{    
    public string currentClass;
    public int currentHealth;

    public TMP_Text nameText;
    public TMP_Text healthText;
    public TMP_Text coinsText;

    public GameObject classIcon, className, showmapbutton;

    private void Start()
    {
        //TODO : Now, all this will be implemented after the websocket is connected
        //GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.AddListener(SetProfileInfo);
        //GameManager.Instance.EVENT_CHARACTERSELECTED.AddListener(SetClassSelected);

        GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(PlayerPrefs.GetString("session_token"));

        //currentClass = PlayerPrefs.GetString("class_selected");
        //SetClassText(currentClass);
        //SetHealth(Random.Range(30, 81));

        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.AddListener(SetProfileInfo);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(SetPlayerState);
    }

    public void OnMapViewTopBar() 
    {      
        showmapbutton.SetActive(false);
    }

    public void OnCombatViewTopBar() 
    {
        showmapbutton.SetActive(true);
    }

    public void SetTextValues(string nameText, string classText, int health, int coins)
   // public void SetTextValues(string nameText, string classText, string healthText, int coins)
    {
        this.nameText.text = nameText;
     
        healthText.text = $"{health} health";
        coinsText.text = $"{coins} coins";
    }

    public void SetNameText(string nameText)
    {
        this.nameText.text = nameText;
    }


    public void SetHealthText(int health, int maxHealth)
    {
        healthText.text = health.ToString() + "/" + maxHealth.ToString();
    }

    public void SetCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    public void SetProfileInfo(ProfileData profileData)
    {
        SetNameText(profileData.data.name);
        SetCoinsText(profileData.data.coins);
    }

    public void SetPlayerState(string data) 
    {
        PlayerStateData playerState = JsonUtility.FromJson<PlayerStateData>(data);
        SetNameText(playerState.data.player_name);
        SetHealthText(playerState.data.hp_current, playerState.data.hp_max);
        SetCoinsText(playerState.data.gold);
    }

    public void SetClassSelected(string classSelected)
    {
        currentClass = classSelected;
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
        //SetHealthText(currentHealth);
    }
    
    public void OnWalletButton()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnSettingsButton()
    {
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }
}