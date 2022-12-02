using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Text volumeText;
    public TMP_Text musicVolumeText;
    public TMP_Text sfxVolumeText;
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
        volumeSlider.value = PlayerPrefs.GetFloat("settings_volume", 1);
        sfxSlider.value = PlayerPrefs.GetFloat("sfx_volume", 1);
        musicSlider.value = PlayerPrefs.GetFloat("music_volume", 0.8f);
        AudioListener.volume = volumeSlider.value;

        languageDropdown.value = PlayerPrefs.GetInt("settings_dropdown");
    }

    public void OnVolumeChanged(string volumeType)
    {
        Slider changedSlider = volumeSlider;
        TMP_Text sliderText = volumeText;
        switch (volumeType)
        {
            case "settings_volume":
                AudioListener.volume = volumeSlider.value;
                break;
            case "sfx_volume":
                changedSlider = sfxSlider;
                sliderText = sfxVolumeText;
                break;
            case "music_volume":
                changedSlider = musicSlider;
                sliderText = musicVolumeText;
                break;
        }

        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        float volumePercent = changedSlider.value * 100;
        string volumeAmount = volumePercent.ToString("F0");
        sliderText.text = (volumeAmount);
        PlayerPrefs.SetFloat(volumeType, changedSlider.value);
        PlayerPrefs.Save();
        GameManager.Instance.EVENT_VOLUME_CHANGED.Invoke();
    }

    public void OnLanguageChanged()
    {
        PlayerPrefs.SetInt("settings_dropdown", languageDropdown.value);
        PlayerPrefs.Save();
    }

    public void OnWalletsButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerSettingsPanel(bool activate)
    {
        settingsContainer.SetActive(activate);
    }

    public void ActivateLogoutConfirmPanel()
    {
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Do you want to logout?",
            () =>
            {
                logoutHyperlink.interactable = false;
                manageWallets.interactable = false;
                GameManager.Instance.EVENT_REQUEST_LOGOUT.Invoke(PlayerPrefs.GetString("session_token"));
            });
    }

    public void OnLogin(string name, int fief)
    {
        logoutHyperlink.interactable = true;
        manageWallets.interactable = true;
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