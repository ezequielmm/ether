using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName="NamedSoundList", menuName = "ScriptableObjects/SoundLists/NamedSoundList")]
public class NamedSoundList : ScriptableObject
{
    [Serializable]
    public struct SoundClip
    {
        public string name;
        [Tooltip("A Random sound will be returned upon query")]
        public List<AudioClip> clips;
    }
    public List<SoundClip> soundClips;
    private Dictionary<string, List<AudioClip>> _soundClipsCache = new();

    public AudioClip GetRandomSound(string soundKey)
    {
        soundKey = soundKey.ToLower();
        if (_soundClipsCache.TryGetValue(soundKey, out var clips))
            return clips[Random.Range(0, clips.Count)];

        var soundClip = soundClips.FirstOrDefault(sc =>
            string.Equals(sc.name, soundKey, StringComparison.OrdinalIgnoreCase));

        if (soundClip.clips == null || soundClip.clips.Count == 0)
            return null;

        return soundClip.clips[Random.Range(0, soundClip.clips.Count)];
    }
}
