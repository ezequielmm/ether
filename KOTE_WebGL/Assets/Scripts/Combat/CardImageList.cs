using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="ImageList", menuName = "ScriptableObjects/CardImageList")]
public class CardImageList : ScriptableObject
{
    public List<Sprite> cardImages;
}
