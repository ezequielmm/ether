using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Dropdown languageDropdown;
    public GameObject settingsContainer;
    public GameObject logoutConfirmContainer;
    public Button logoutHyperlink, manageWallets;

    private void Start()
    {
        settingsContainer.SetActive(false);

        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerSettingsPanel);

        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.AddListener(OnLogin);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_ERROR.AddListener(OnLogoutError);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        logoutHyperlink.interactable = false;

       // Debug.Log($"{PlayerPrefs.GetFloat("settings_volume")}");
        volumeSlider.value = PlayerPrefs.GetFloat("settings_volume");
        AudioListener.volume = volumeSlider.value;

        languageDropdown.value = PlayerPrefs.GetInt("settings_dropdown");
    }

    public void OnVolumeChanged()
    {
        AudioListener.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("settings_volume", AudioListener.volume);
        PlayerPrefs.Save();
    }

    public void OnLanguageChanged()
    {
        PlayerPrefs.SetInt("settings_dropdown", languageDropdown.value);
        PlayerPrefs.Save();
    }

    public void OnWalletsButton()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerSettingsPanel(bool activate)
    {
        settingsContainer.SetActive(activate);
    }

    public void ActivateInnerLogoutConfirmPanel(bool activate)
    {
        logoutConfirmContainer.SetActive(activate);
    }

    public void OnLogin(string name, int fief)
    {
        logoutHyperlink.interactable = true;
        manageWallets.interactable = true;
    }

    public void OnLogout()
    {
        logoutHyperlink.interactable = false;
        manageWallets.interactable = false;
        GameManager.Instance.EVENT_REQUEST_LOGOUT.Invoke(PlayerPrefs.GetString("session_token"));
    }

    public void OnLogoutError(string errorMessage)
    {
        Debug.Log($"Logout error: {errorMessage}");
    }

    public void OnLogoutSuccessful(string message)
    {
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();

        Debug.Log($"{message}");
    }
}