using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="SpriteList", menuName = "ScriptableObjects/SpriteList")]
public class SpriteList : ScriptableObject
{
    [FormerlySerializedAs("cardImages")] public List<Sprite> entityImages;
}
