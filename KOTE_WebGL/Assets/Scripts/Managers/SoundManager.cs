using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    SettingsManager settingsManager;

    [Tooltip("A List of sounds linked to a name. [case insensitive]")] [SerializeField]
    private NamedSoundList KnightSounds;

    [SerializeField] private NamedSoundList EnemyDefensiveSounds;
    [SerializeField] private NamedSoundList EnemyOffensiveSounds;
    [SerializeField] private NamedSoundList CardSounds;
    [SerializeField] private NamedSoundList UiSounds;

    [SerializeField] bool showSoundDebugs = false;

    float MasterVolume => PlayerPrefs.GetFloat("settings_volume");

    public void Start()
    {
        GameManager.Instance.EVENT_PLAY_SFX.AddListener(PlaySfx);
    }

    public void PlaySfx(SoundTypes soundType, string sound)
    {
        PlaySfx(soundType, sound, Vector2.zero);
    }

    public void PlaySfx(SoundTypes soundType, string sound, Vector2 location)
    {
        AudioClip clip = GetAudioClip(soundType, sound);
        if (clip == null)
        {
            Debug.LogError(
                $"[Sound Manager] Audio clip for \"{sound}\" could not be found. Make sure you spelled it right and make sure the clip exists in the Sound Manager.");
            return;
        }

        if (showSoundDebugs)
        {
            Debug.Log($"[Sound Manager] Playing Sound: {sound}");
        }

        AudioSource.PlayClipAtPoint(clip, location, MasterVolume);
    }

    private AudioClip GetAudioClip(SoundTypes soundType, string soundName)
    {
        List<NamedSoundList.SoundClip> soundClipList;
        switch (soundType)
        {
            case SoundTypes.Card:
                soundClipList = CardSounds.soundClips;
                break;
            case SoundTypes.EnemyOffensive:
                soundClipList = EnemyOffensiveSounds.soundClips;
                break;
            case SoundTypes.EnemyDefensive:
                soundClipList = EnemyDefensiveSounds.soundClips;
                break;
            case SoundTypes.Knight:
                soundClipList = KnightSounds.soundClips;
                break;
            case SoundTypes.UI:
                soundClipList = UiSounds.soundClips;
                break;
            default:
                Debug.LogError($"No Sound List Found for Sound Type {soundType}");
                return null;
        }

        NamedSoundList.SoundClip relatedClip =
            soundClipList.FirstOrDefault(sc => sc.name.ToLower() == soundName.ToLower());

        if (relatedClip.clips == null || relatedClip.clips.Count == 0)
        {
            Debug.LogError($"No Audio Clips for Sound Type {soundType} and Sound Name {soundName}");
            return null;
        }

        return relatedClip.clips[Random.Range(0, relatedClip.clips.Count)];
    }
}