using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour
{
    private Trinket trinket;

    [Serializable]
    public struct TrinketImage
    {
        public String trinketName;
        public Sprite trinketSprite;
    }

    public List<TrinketImage> imageList;
    public Image trinketImage;

    // uses string values for right now, will probably need to change once we parse trinket data
    public void Populate(Trinket data)
    {
        trinket = data;

        //TODO determine how to populate the trinket data
        TrinketImage correctImg = imageList.Find(image => image.trinketName == trinket.name);
        trinketImage.sprite = correctImg.trinketSprite;
    }
}