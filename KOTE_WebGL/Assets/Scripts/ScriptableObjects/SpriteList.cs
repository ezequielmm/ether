using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName="SpriteList", menuName = "ScriptableObjects/SpriteLists/SpriteList")]
public class SpriteList : ScriptableObject
{
    [FormerlySerializedAs("cardImages")]
    public List<Sprite> entityImages;
    public List<AssetReference> entityImagesRef;
}

