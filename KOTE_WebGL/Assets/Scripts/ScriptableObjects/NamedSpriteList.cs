using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="NamedSpriteList", menuName = "ScriptableObjects/SpriteLists/NamedSpriteList")]
public class NamedSpriteList : ScriptableObject
{
    [Serializable]
    public struct NamedSprite
    {
        public string name;
        public Sprite image;
    }

    [FormerlySerializedAs("NamedSpriteList")] public List<NamedSprite> SpriteList;
}
