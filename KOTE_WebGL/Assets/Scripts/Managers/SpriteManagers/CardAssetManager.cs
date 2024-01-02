using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardAssetManager : SingleTon<CardAssetManager>
{
    public Sprite defaultImage;

    public NamedSpriteList banners;
    public NamedSpriteList frames;
    public NamedSpriteList gems;

    public NamedSpriteList cardImages;
    private Dictionary<int, Sprite> cardImageDict;

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
        cardImageDict ??= GenerateCardImagesDictionary();
        
        var cardImage = cardImageDict.GetValueOrDefault(cardId);
        if (cardImage != null)
            return cardImage;
        
        // Upgraded version
        cardImage = cardImageDict.GetValueOrDefault(cardId - 1);
        if (cardImage != null)
            return cardImage;
        
        return defaultImage;
    }

    private Dictionary<int, Sprite> GenerateCardImagesDictionary() =>
        // Ignore those who can't parse to int
        cardImages.SpriteList.Where(e => int.TryParse(e.name, out _))
            .ToDictionary(e => int.Parse(e.name), e => e.image);
}
