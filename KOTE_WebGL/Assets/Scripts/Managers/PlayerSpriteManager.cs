using System;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    // we need the skeleton data so we can pull the image assets without needing the player to be activated in combat
    public SkeletonDataAsset KinghtData;
    private List<Trait> currentMetadata = new List<Trait>();

    private List<TraitSprite> SkinSprites = new List<TraitSprite>();
    private List<TraitSprite> DefaultSprites = new List<TraitSprite>();

    // these are for keeping track of the number of requests made, just in case it takes longer than expected to pull the images
    private int completedSkinRequests = 0;
    private int skinRequestsMade = 0;
    private SkeletonData knightSkeletonData;

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.AddListener(OnSkinSpriteReceived);
        GameManager.Instance.EVENT_NFT_SKIN_SPRITE_FAILED.AddListener(OnSkinSpriteFailed);
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(OnNftSelected);
        knightSkeletonData = KinghtData.GetSkeletonData(true);
        RequestDefaultSkins();
    }

    private void RequestDefaultSkins()
    {
        for (int i = 0; i < GameSettings.DEFAULT_SKIN_DATA.Length; i++)
        {
            TraitSprite trait = GameSettings.DEFAULT_SKIN_DATA[i];
            Skin traitSkin = knightSkeletonData.Skins.Find(x => x.Name == trait.skinName);
            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                string[] baseImageName = skinEntry.Attachment.Name.Split('/');
                string imageName = (baseImageName.Length > 1) ? baseImageName[1] : baseImageName[0];
                imageName = imageName.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                if (imageName.Contains("placeholder")) continue;
                // pack this all as a single object to make Events and transferring the information easier

                trait.attachmentIndex = skinEntry.SlotIndex;
                trait.imageName = imageName;
                trait.isDefault = true;


                skinRequestsMade++;
                GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(trait);
            }
        }
    }

    private void OnNftSelected(NftMetaData nftMetaData)
    {
        currentMetadata.Clear();
        currentMetadata.AddRange(nftMetaData.traits);
        foreach (Trait trait in currentMetadata)
        {
            string[] traitSplit = trait.value.Split();
            Skin traitSkin = knightSkeletonData.Skins.Find(x => x.Name.Contains(trait.trait_type + "_" +string.Join('_', traitSplit)));
            if (traitSkin == null) continue;
            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                string[] baseImageName = skinEntry.Attachment.Name.Split('/');
                string imageName = (baseImageName.Length > 1) ? baseImageName[1] : baseImageName[0];
                imageName = imageName.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                if (imageName.Contains("placeholder")) continue;
                // pack this all as a single object to make Events and transferring the information easier
                TraitSprite tempTraitSprite = new TraitSprite
                {
                    skinName = traitSkin.Name,
                    traitType = trait.trait_type,
                    attachmentIndex = skinEntry.SlotIndex,
                    imageName = imageName
                };

                GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(tempTraitSprite);
                skinRequestsMade++;
            }
        }
        Debug.Log($"[PlayerSpriteManager] Nft #{nftMetaData.token_id} has been selected.");
    }

    private void OnSkinSpriteReceived(TraitSprite receivedSprite)
    {
        if (receivedSprite.isDefault)
        {
            AddDefaultSkin(receivedSprite);
        }
        else
        {
           AddNftSkin(receivedSprite); 
        }
        
        completedSkinRequests++;
        CheckIfSkinRequestsCompleted();
    }

    private void AddDefaultSkin(TraitSprite receivedSprite)
    {
        DefaultSprites.Add(receivedSprite);
    }

    private void AddNftSkin(TraitSprite receivedSprite)
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
            Debug.Log($"[PlayerSpriteManager] Skin Images have been loaded!");
            completedSkinRequests = 0;
            skinRequestsMade = 0;
            GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.Invoke();
        }
    }

    // get the list of all skins for the knight, using the defaults if it doesn't exist
    public List<TraitSprite> GetAllTraitSprites()
    {
        List<TraitSprite> allSprites = new List<TraitSprite>();
        foreach (var traitType in Enum.GetNames(typeof(TraitTypes)))
        {
            if (SkinSprites.Exists(x => x.traitType == traitType))
            {
                allSprites.AddRange(SkinSprites.FindAll(x => x.traitType == traitType));
                continue;
            }

            if (traitType == nameof(TraitTypes.Sigil) || traitType == nameof(TraitTypes.Crest)
                                                      || traitType == nameof(TraitTypes.Vambrace))
            {
                continue;
            }

            if (DefaultSprites.Exists(x => x.traitType == traitType))
            {
                allSprites.AddRange(DefaultSprites.FindAll(x => x.traitType == traitType));
            }
        }
        
        return allSprites;
    }
}