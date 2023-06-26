using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="NamedSpriteList", menuName = "ScriptableObjects/SpriteLists/NamedSpriteList")]
public class NamedSpriteList : ScriptableObject
{
    [Serializable]
    public class NamedSprite
    {
        public string name;
        public Sprite image;
        public AssetReference imageRef;
    }

    [FormerlySerializedAs("NamedSpriteList")] public List<NamedSprite> SpriteList;
}
