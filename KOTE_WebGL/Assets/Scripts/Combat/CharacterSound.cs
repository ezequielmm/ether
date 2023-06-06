using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterSound : MonoBehaviour
{
    [Serializable]
    public struct SoundListMapper
    {
        public string Name;
        public AudioClip[] SoundList;
    }
    [SerializeField]
    private List<SoundListMapper> _soundsListMap = new ()
    {
        new SoundListMapper{Name = "Attack", SoundList = Array.Empty<AudioClip>()},
        new SoundListMapper{Name = "Hit", SoundList = Array.Empty<AudioClip>()},
        new SoundListMapper{Name = "Death", SoundList = Array.Empty<AudioClip>()},
        new SoundListMapper{Name = "Cast", SoundList = Array.Empty<AudioClip>()},
    };
    private Dictionary<string, AudioClip[]> _soundDictionary = new ();
    
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _soundDictionary = _soundsListMap.ToDictionary(k => k.Name, v => v.SoundList);
    }
    
    public void PlaySound(string sound)
    {
        Debug.Log($"PlayAudiO   {sound}");
        var clip = GetAudioClip(sound);
        if (clip == null)
            return;
        
        _audioSource.PlayOneShot(clip);
    }

    private AudioClip GetAudioClip(string sound)
    {
        if (string.IsNullOrEmpty(sound) || !_soundDictionary.ContainsKey(sound)) {
            Debug.LogWarning($"[CharacterSound] Sound \"{sound}\" could not be found.");
            return null;
        }
        var clips = _soundDictionary[sound];
        if (clips.Length != 0)
            return clips[Random.Range(0, clips.Length)];
        
        Debug.LogWarning($"[Sound Manager] Sound \"{sound}\" has no clips.");
        return null;
    }
}
