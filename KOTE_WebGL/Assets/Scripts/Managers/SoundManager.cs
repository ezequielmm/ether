using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : SingleTon<SoundManager>
{
    SettingsManager settingsManager;

    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioSource SfxSource;
    [SerializeField] private List<NamedSoundList> BackgroundMusic;
    [SerializeField] private NamedSoundList KnightSounds;
    [SerializeField] private NamedSoundList EnemyDefensiveSounds;
    [SerializeField] private NamedSoundList EnemyOffensiveSounds;
    [SerializeField] private NamedSoundList CardSounds;
    [SerializeField] private NamedSoundList UiSounds;

    [SerializeField] bool showSoundDebugs = false;

    private float SfxVolume => PlayerPrefs.GetFloat("sfx_volume", 1);
    private float MusicVolume => PlayerPrefs.GetFloat("music_volume", 0.5f);

    public void Start()
    {
        GameManager.Instance.EVENT_PLAY_SFX.AddListener(PlaySfx);
        GameManager.Instance.EVENT_PLAY_MUSIC.AddListener(PlayMusic);
        GameManager.Instance.EVENT_VOLUME_CHANGED.AddListener(UpdateVolume);
        MusicSource.volume = MusicVolume;
        SfxSource.volume = SfxVolume;
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
        
        SfxSource.PlayOneShot(clip);
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

    private void PlayMusic(MusicTypes type, int act)
    {
        List<NamedSoundList.SoundClip> musicList = BackgroundMusic[act].soundClips;
        if (musicList.Exists(x => x.name == type.ToString()))
        {
            AudioClip music = musicList.Find(x => x.name == type.ToString()).clips[0];
            MusicSource.clip = music;
            MusicSource.Play();
        }
    }

    private void UpdateVolume()
    {
        MusicSource.volume = MusicVolume;
        SfxSource.volume = SfxVolume;
    }
}