using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="CardSpriteList", menuName = "ScriptableObjects/CardSpriteList")]
public class CardSpriteList : ScriptableObject
{
    [Serializable]
    public struct NamedSprite
    {
        public string name;
        public Sprite image;
    }

    public List<NamedSprite> NamedSpriteList;
}
