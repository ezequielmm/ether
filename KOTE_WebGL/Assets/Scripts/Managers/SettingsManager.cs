using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public GameObject settingsContainer;

    private void Start()
    {
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerSettingsPanel);

        GameManager.Instance.EVENT_REQUEST_LOGOUT_ERROR.AddListener(OnLogoutError);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);

        volumeSlider.value = AudioListener.volume;
    }

    public void OnVolumeChanged()
    {
        AudioListener.volume = volumeSlider.value;
        // Debug.Log($"Volume = {AudioListener.volume * 100}");
    }

    public void OnWalletsButton()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerSettingsPanel(bool activate)
    {
        settingsContainer.SetActive(activate);
    }

    public void OnLogout()
    {
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