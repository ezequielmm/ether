using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour
{
    [Serializable]
    public struct TrinketImage
    {
        public String trinketName;
        public Sprite trinketSprite;
    }

    public List<TrinketImage> imageList;
    public Image trinketImage;
    private string name;
    private string rarity;

    // uses string values for right now, will probably need to change once we parse trinket data
    public void Populate(string trinketName, string trinketRarity)
    {
        //TODO determine how to populate the trinket data
        name = trinketName;
        rarity = trinketRarity;
        TrinketImage correctImg = imageList.Find(image => image.trinketName == name);
        trinketImage.sprite = correctImg.trinketSprite;
    }
}