using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    SettingsManager settingsManager;

    [Tooltip("A List of sounds linked to a name. [case insensitive]")]
    [SerializeField] List<SoundClip> soundClipList;

    float MasterVolume => PlayerPrefs.GetFloat("settings_volume");

    [System.Serializable]
    private class SoundClip
    {
        public string name;
        public AudioClip clip;
    }


    public void PlaySfx(string sound)
    {
        PlaySfx(sound, Vector2.zero);
    }

    public void PlaySfx(string sound, Vector2 location) 
    {
        AudioClip clip = GetAudioClip(sound);
        if (clip == null) 
        {
            Debug.LogError($"[Sound Manager] Audio clip for \"{sound}\" could not be found. Make sure you spelled it right and make sure the clip exists in the Sound Manager.");
            return;
        }
        AudioSource.PlayClipAtPoint(clip, location, MasterVolume);
    }

    private AudioClip GetAudioClip(string soundName) 
    {
        SoundClip relatedClip = soundClipList.FirstOrDefault(sc => sc.name.ToLower() == soundName.ToLower());
        if (relatedClip == null) 
        {
            return null;
        }
        return relatedClip.clip;
    }
}