using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PortraitSpriteManager : SingleTon<PortraitSpriteManager>
{
    private Dictionary<Trait, Dictionary<string, Sprite>> villagerCache = new();
    private Dictionary<int, Sprite> knightCache = new();

    public async void CacheAllSprites()
    {
        List<Nft> nfts = NftManager.Instance.GetAllNfts();

        foreach (Nft nft in nfts)
        {
            if(nft.isKnight)
            {
                await GetKnightPortrait(nft);
                continue;
            }
            foreach (KeyValuePair<Trait,string> traitPair in nft.Traits)
            {
                if (traitPair.Value.Contains("None")) continue;
                await GetPortraitSprite(traitPair.Key, traitPair.Value);
            }
        }
    }

    public async UniTask<Sprite> GetKnightPortrait(Nft nft)
    {
        if (knightCache.ContainsKey(nft.TokenId)) return knightCache[nft.TokenId];
        Sprite nftPortrait = await nft.GetImage();
        knightCache[nft.TokenId] = nftPortrait;
        return nftPortrait;
    }
    
    public async UniTask<Sprite> GetPortraitSprite(Trait trait, string spriteName)
    {
        if (villagerCache.ContainsKey(trait) && villagerCache[trait].ContainsKey(spriteName))
        {
            return villagerCache[trait][spriteName];
        }

        return await RequestPortraitSprite(trait, spriteName);
    }

    

    private async UniTask<Sprite> RequestPortraitSprite(Trait trait, string spriteName)
    {
        Texture2D spriteTexture = await FetchData.Instance.GetVillagerPortraitElement($"{trait}/{spriteName}");
        if (spriteTexture == null)
        {
            Debug.LogWarning($"Portrait image not found for {trait} {spriteName}");
            return null;
        }

        CacheSprite(spriteTexture, trait, spriteName);
        return villagerCache[trait][spriteName];
    }

    private void CacheSprite(Texture2D spriteTexture, Trait trait, string spriteName)
    {
        if (villagerCache.ContainsKey(trait))
        {
            villagerCache[trait][spriteName] = spriteTexture.ToSprite();
        }
        else
        {
            villagerCache[trait] = new Dictionary<string, Sprite>();
            villagerCache[trait][spriteName] = spriteTexture.ToSprite();
        }
    }
}
