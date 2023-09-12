using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="ActBackgroundsList", menuName = "ScriptableObjects/ActBackgrounds/ActBackgroundsList")]
public class ActBackgroundsList : ScriptableObject
{
    [Serializable]
    public struct ActBackground
    {
        public int Step;
        public GameObject Background;
    }

    public List<ActBackground> BackgroundsList;
    
    public GameObject BossBackground;
}