using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAssetManager : SingleTon<SpriteAssetManager>
{
    public Sprite defaultImage;
    public List<SpriteList> potionImageList;
    public NamedSpriteList miscImages;
    private List<(int, int)> _potionListRanges = new List<(int, int)>();

    private void Start()
    {
        // the purpose of this Start() is to cache the image range for any item that has multiple lists of images

        // cache the range of potion ids to check against when a card is asked for
        foreach (SpriteList imageList in potionImageList)
        {
            _potionListRanges.Add((int.Parse(imageList.entityImages[0].name),
                int.Parse(imageList.entityImages[imageList.entityImages.Count - 1].name)));
        }
    }

    public Sprite GetPotionImage(int potionId)
    {
        for (int i = 0; i < potionImageList.Count; i++)
        {
            var range = _potionListRanges[i];
            if (potionId < range.Item1 || potionId > range.Item2)
            {
                continue;
            }

            List<Sprite> cardImages = potionImageList[i].entityImages;
            if (cardImages.Exists(image => int.Parse(image.name) == potionId))
            {
                return cardImages.Find(image => int.Parse(image.name) == potionId);
            }
        }

        Debug.LogWarning($"No potion image for potion ID {potionId} found. You probably need to pester the backend");
        return defaultImage;
    }

    public Sprite GetMiscImage(string imageName)
    {
        if (miscImages.SpriteList.Exists(item =>
                item.name.Equals(imageName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return miscImages.SpriteList
                .Find(item => item.name.Equals(imageName, StringComparison.CurrentCultureIgnoreCase)).image;
        }

        Debug.LogWarning($"no misc image with name {imageName} was found. Check that you are using a valid image name");
        return defaultImage;
    }
}