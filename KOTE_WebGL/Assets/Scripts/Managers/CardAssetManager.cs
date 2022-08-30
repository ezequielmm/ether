using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAssetManager : SingleTon<CardAssetManager>
{
    public Sprite defaultImage;

    public CardSpriteList banners;
    public CardSpriteList frames;
    public CardSpriteList gems;
    [Tooltip("Place all CardImageList object here to populate the card images")]
    public List<CardImageList> cardImageLists;

    // store the start and end id of the cards in the list for faster referencing
    private List<(int, int)> imageListRanges = new List<(int, int)>();

    private void Start()
    {
        // cache the range of card ids to check against when a card is asked for
        foreach (CardImageList imageList in cardImageLists)
        {
            imageListRanges.Add((int.Parse(imageList.cardImages[0].name),
                int.Parse(imageList.cardImages[imageList.cardImages.Count-1].name)));
        }
    }

    public Sprite GetGem(string cardType)
    {
        return gems.NamedSpriteList.Find(gem => gem.name == cardType).image;
    }

    public Sprite GetFrame(string cardPool)
    {
        return frames.NamedSpriteList.Find(frame => frame.name == cardPool).image;
    }

    public Sprite GetBanner(string cardRarity)
    {
        return banners.NamedSpriteList.Find(banner => banner.name == cardRarity).image;
    }
    
    public Sprite GetCardImage(int cardId)
    {
        
        for (int i = 0; i < cardImageLists.Count; i++)
        {
            var range = imageListRanges[i];
            if (cardId < range.Item1 || cardId > range.Item2)
            {
                continue;
            }

            List<Sprite> cardImages = cardImageLists[i].cardImages;
            if (cardImages.Exists(image => int.Parse(image.name) == cardId || int.Parse(image.name) + 1 == cardId))
            {
                return cardImages.Find(image => int.Parse(image.name) == cardId || int.Parse(image.name) + 1 == cardId);
            }
        }
        
        return defaultImage;
    }
}
