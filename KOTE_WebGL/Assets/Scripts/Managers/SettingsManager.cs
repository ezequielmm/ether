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
        volumeSlider.value = AudioListener.volume;
    }

    public void OnVolumeChanged()
    {
        AudioListener.volume = volumeSlider.value;
        Debug.Log($"Volume = {AudioListener.volume * 100}");
    }

    public void ActivateInnerSettingsPanel(bool activate)
    {
        settingsContainer.SetActive(activate);
    }
}