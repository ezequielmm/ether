using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
