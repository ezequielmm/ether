using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    AudioSource audioSource;
    SettingsManager settingsManager;

    float MasterVolume => PlayerPrefs.GetFloat("settings_volume");


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
}
