using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine;
using UnityEngine;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public List<TraitSprite> SkinSprites = new List<TraitSprite>();
    private List<Trait> currentMetadata = new List<Trait>();

    private int completedSkinRequests = 0;
    private int skinRequestsMade = 0;

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(OnNftMetadataReceived);
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.AddListener(OnSkinSpriteReceived);
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_FAILED.AddListener(OnSkinSpriteFailed);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(2702);
    }

    public Sprite GetSpriteForTrait(string traitType)
    {
        if (SkinSprites.Exists(x => x.trait.trait_type == traitType))
        {
            return SkinSprites.Find(x => x.trait.trait_type == traitType).sprite;
        }

        
        return null;
    }

    private void OnNftMetadataReceived(NftMetaData nftMetaData)
    {
        currentMetadata.Clear();
        currentMetadata.AddRange(nftMetaData.traits);
        completedSkinRequests = 0;
        skinRequestsMade = 0;
        foreach (Trait trait in currentMetadata)
        {
            if (trait.trait_type == "Background") continue;
            GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(trait);
            skinRequestsMade++;
        }
    }

    private void OnSkinSpriteReceived(Sprite skinSprite, Trait skinTrait)
    {
        if (SkinSprites.Exists(x => x.trait.trait_type == skinTrait.trait_type))
        {
            TraitSprite traitSprite = SkinSprites.Find(x => x.trait.trait_type == skinTrait.trait_type);
            traitSprite.trait = skinTrait;
            traitSprite.sprite = skinSprite;
        }
        else
        {
            SkinSprites.Add(new TraitSprite
            {
                trait = skinTrait,
                sprite = skinSprite
            });
        }
        completedSkinRequests++;
        CheckIfSkinRequestsCompleted();
    }

    private void OnSkinSpriteFailed()
    {
        completedSkinRequests++;
        CheckIfSkinRequestsCompleted();
    }

    private void CheckIfSkinRequestsCompleted()
    {
        if (completedSkinRequests >= skinRequestsMade)
        {
            completedSkinRequests = 0;
            GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.Invoke();
        }
    }
}

[Serializable]
public struct TraitSprite
{
    public Trait trait;
    public Sprite sprite;
}