using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public List<TraitSprite> SkinSprites = new List<TraitSprite>();
    // we need the skeleton data so we can pull the image assets without needing the player to be activated in combat
    public SkeletonDataAsset KinghtData;
    private List<Trait> currentMetadata = new List<Trait>();

    // these are for keeping track of the number of requests made, just in case it takes longer than expected to pull the images
    private int completedSkinRequests = 0;
    private int skinRequestsMade = 0;
    private SkeletonData knightSkeletonData;

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(OnNftMetadataReceived);
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.AddListener(OnSkinSpriteReceived);
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_FAILED.AddListener(OnSkinSpriteFailed);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(2702);
        knightSkeletonData = KinghtData.GetSkeletonData(true);
    }

    private void OnNftMetadataReceived(NftMetaData nftMetaData)
    {
        currentMetadata.Clear();
        currentMetadata.AddRange(nftMetaData.traits);
        foreach (Trait trait in currentMetadata)
        {
            string[] traitSplit = trait.value.Split();
            Skin traitSkin = knightSkeletonData.Skins.Find(x => x.Name.Contains(string.Join('_', traitSplit)));
            if(traitSkin == null) continue;
            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                string[] baseImageName = skinEntry.Attachment.Name.Split('/');
                string imageName = (baseImageName.Length > 1) ? baseImageName[1] : baseImageName[0];
                
                // pack this all as a single object to make Events and transferring the information easier
                TraitSprite tempTraitSprite = new TraitSprite
                {
                    traitType = trait.trait_type,
                    attachmentIndex = skinEntry.SlotIndex,
                    imageName = imageName
                };
                
                GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(tempTraitSprite);
                skinRequestsMade++;
            } 
        }
    }

    private void OnSkinSpriteReceived(TraitSprite receivedSprite)
    {
        if (SkinSprites.Exists(x => x.attachmentIndex == receivedSprite.attachmentIndex))
        {
            TraitSprite traitSprite = SkinSprites.Find(x => x.attachmentIndex == receivedSprite.attachmentIndex);
            SkinSprites[SkinSprites.IndexOf(traitSprite)] = receivedSprite;
        }
        else
        {
            SkinSprites.Add(receivedSprite);
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

    public Sprite GetSpriteForAttachment(int attachmentIndex)
    {
        if (SkinSprites.Exists(x => x.attachmentIndex == attachmentIndex))
        {
            return SkinSprites.Find(x => x.attachmentIndex == attachmentIndex).sprite;
        }
        return null;
    }
}

