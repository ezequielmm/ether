using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class SoundManager : SingleTon<SoundManager>
{
    SettingsManager settingsManager;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioSource SfxSource;
    [SerializeField] private AudioSource AmbienceSource;
    [SerializeField] private List<NamedSoundList> BackgroundMusic;

    [SerializeField] private List<SoundListMapper> _soundsListMap;
    [System.Serializable]
    private struct SoundListMapper {
        public SoundTypes Type;
        public NamedSoundList SoundList;
    }
    private Dictionary<SoundTypes, NamedSoundList> _soundDictionary = new();

    [SerializeField] bool showSoundDebugs = true;

    private float SfxVolume => PlayerPrefs.GetFloat("sfx_volume", 1);
    private float MusicVolume => PlayerPrefs.GetFloat("music_volume", 0.5f);

    protected override void Awake()
    {
        base.Awake();
        _soundDictionary = _soundsListMap.ToDictionary(k => k.Type, v => v.SoundList);
    }

    public void Start()
    {
        GameManager.Instance.EVENT_PLAY_SFX.AddListener(PlaySfx);
        GameManager.Instance.EVENT_PLAY_MUSIC.AddListener(OnPlayMusic);
        GameManager.Instance.EVENT_VOLUME_CHANGED.AddListener(UpdateVolume);
        GameManager.Instance.EVENT_STOP_MUSIC.AddListener(StopMusic);
        UpdateVolume();
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
            Debug.LogWarning(
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
        var clip = _soundDictionary[soundType].GetRandomSound(soundName);

        if (clip != null)
            return clip;
        
        Debug.LogError($"No Audio Clips for Sound Type {soundType} and Sound Name {soundName}");
        return null;
    }

    private void OnPlayMusic(MusicTypes type, int act)
    {
        switch (type)
        {
            case MusicTypes.Boss:
            case MusicTypes.Music:
                PlayMusic(type, act);
                break;
            case MusicTypes.Ambient:
                PlayAmbientSound(act);
                break;
        }
    }

    private void PlayMusic(MusicTypes type, int act)
    {
        List<NamedSoundList.SoundClip> musicList = BackgroundMusic[act].soundClips;
        if (musicList.Exists(x => x.name == type.ToString()))
        {
            AudioClip music = musicList.Find(x => x.name == type.ToString()).clips[0];
            if (music != MusicSource.clip || MusicSource.isPlaying == false)
            {
                MusicSource.clip = music;
                MusicSource.Play();
            }
        }
    }

    private void StopMusic()
    {
        MusicSource.DOFade(0, 0.5f).OnComplete(() =>
        {
            MusicSource.Stop();
            MusicSource.volume = MusicVolume;
        });
        AmbienceSource.DOFade(0, 0.5f).OnComplete(() =>
        {
            AmbienceSource.Stop();
            AmbienceSource.volume = SfxVolume;
        });;
    }

    private void PlayAmbientSound(int act)
    {
        List<NamedSoundList.SoundClip> musicList = BackgroundMusic[act].soundClips;
        if (musicList.Exists(x => x.name == "Ambient"))
        {
            AudioClip music = musicList.Find(x => x.name == "Ambient").clips[0];
            if (music != AmbienceSource.clip || AmbienceSource.isPlaying == false)
            {
                AmbienceSource.clip = music;
                AmbienceSource.Play();
            }
        }
    }

    private void UpdateVolume()
    {
        audioMixer.SetFloat("music_volume", ConvertToDecibels(MusicVolume));
        audioMixer.SetFloat("sfx_volume", ConvertToDecibels(SfxVolume));
        audioMixer.SetFloat("ambience_volume", ConvertToDecibels(SfxVolume));
        // MusicSource.volume = MusicVolume;
        // SfxSource.volume = SfxVolume;
        // AmbienceSource.volume = SfxVolume;
    }
    private float ConvertToDecibels(float sliderValue)
    {
        if (sliderValue != 0)
            return 20.0f * Mathf.Log10(sliderValue);
        return -80.0f;
    }
}