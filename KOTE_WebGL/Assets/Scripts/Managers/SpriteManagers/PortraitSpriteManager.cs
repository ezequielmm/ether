using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PortraitSpriteManager : SingleTon<PortraitSpriteManager>
{
    private Dictionary<Trait, Dictionary<string, Sprite>> spriteCache = new();

    public async void CacheAllSprites()
    {
        List<Nft> nfts = NftManager.Instance.GetAllNfts();

        foreach (Nft nft in nfts)
        {
            foreach (KeyValuePair<Trait,string> traitPair in nft.Traits)
            {
                if (traitPair.Value.Contains("None")) continue;
                await GetPortraitSprite(traitPair.Key, traitPair.Value);
            }
        }
    }
    
    public async UniTask<Sprite> GetPortraitSprite(Trait trait, string spriteName)
    {
        if (spriteCache.ContainsKey(trait) && spriteCache[trait].ContainsKey(spriteName))
        {
            return spriteCache[trait][spriteName];
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
        return spriteCache[trait][spriteName];
    }

    private void CacheSprite(Texture2D spriteTexture, Trait trait, string spriteName)
    {
        if (spriteCache.ContainsKey(trait))
        {
            spriteCache[trait][spriteName] = spriteTexture.ToSprite();
        }
        else
        {
            spriteCache[trait] = new Dictionary<string, Sprite>();
            spriteCache[trait][spriteName] = spriteTexture.ToSprite();
        }
    }
}
