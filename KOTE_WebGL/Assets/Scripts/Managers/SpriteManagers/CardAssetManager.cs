using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CardAssetManager : SingleTon<CardAssetManager>
{
    public Sprite defaultImage;

    public NamedSpriteList banners;
    public NamedSpriteList frames;
    public NamedSpriteList gems;
    [Tooltip("Place all CardImageList object here to populate the card images")]
    public List<SpriteList> cardImageLists;

    // store the start and end id of the cards in the list for faster referencing
    private List<(int, int)> imageListRanges = new List<(int, int)>();

    private void Start()
    {
        Debug.Log("cardimagelists:" + cardImageLists.Count);
        // cache the range of card ids to check against when a card is asked for
        foreach (SpriteList imageList in cardImageLists)
        {
            var minRange = int.Parse(imageList.entityImages[0].name);
            var maxRange = imageList.entityImages.Select(e => e.name).Select(int.Parse).Max();

            Debug.Log("cardimagelists: " + minRange + " cardimagelists: " + maxRange);
                 
            imageListRanges.Add((minRange, maxRange));
        }
    }

    public Sprite GetGem(string cardType, bool isUpgraded)
    {
        if (isUpgraded) cardType += "Upgraded";
        return gems.SpriteList.Find(gem => gem.name == cardType).image;
    }

    public Sprite GetFrame(string cardPool)
    {
        return frames.SpriteList.Find(frame => frame.name == cardPool).image;
    }

    public Sprite GetBanner(string cardRarity)
    {
        return banners.SpriteList.Find(banner => banner.name == cardRarity).image;
    }
    
    public Sprite GetCardImage(int cardId)
    {
        
        for (int i = 0; i < cardImageLists.Count; i++)
        {
            var range = imageListRanges[i];
            if (cardId < range.Item1 || cardId > range.Item2 + 1) // +1 for the upgraded version
            {
                continue;
            }

            List<Sprite> cardImages = cardImageLists[i].entityImages;
            if (cardImages.Exists(image => int.Parse(image.name) == cardId || int.Parse(image.name) + 1 == cardId))
            {
                return cardImages.Find(image => int.Parse(image.name) == cardId || int.Parse(image.name) + 1 == cardId);
            }
        }
        
        return defaultImage;
    }
}
