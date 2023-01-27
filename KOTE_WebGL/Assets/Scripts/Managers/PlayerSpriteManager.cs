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
    public SpriteList DefaultSkinImages;
    private List<Trait> currentMetadata = new List<Trait>();

    private List<TraitSprite> SkinSprites = new List<TraitSprite>();
    private List<TraitSprite> DefaultSprites = new List<TraitSprite>();

    // these are for keeping track of the number of requests made, just in case it takes longer than expected to pull the images
    private int completedSkinRequests = 0;
    private int skinRequestsMade = 0;
    private SkeletonData knightSkeletonData;
    private string shieldType = "";

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
                if (DefaultSkinImages.entityImages.Exists(x => x.name == trait.imageName))
                {
                    trait.sprite = DefaultSkinImages.entityImages.Find(x => x.name == trait.imageName);
                    GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.Invoke(trait);
                }
                else
                {
                    GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(trait);
                }
            }
        }
    }

    private void OnNftSelected(NftMetaData nftMetaData)
    {
        currentMetadata.Clear();
        currentMetadata.AddRange(nftMetaData.traits);

        // ===============TEMP CODE FOR BOTTOMS HANDLING=====================================
        if (currentMetadata.Exists(x => x.trait_type == nameof(TraitTypes.Gauntlet)))
        {
            Trait tempTrait = currentMetadata.Find(x => x.trait_type == nameof(TraitTypes.Gauntlet));
            Trait bottomTrait = SelectTraitBottoms(tempTrait);
            if (bottomTrait != null) currentMetadata.Add(bottomTrait);
        }

        if (currentMetadata.Exists(x => x.trait_type == nameof(TraitTypes.Breastplate)))
        {
            Trait tempTrait = currentMetadata.Find(x => x.trait_type == nameof(TraitTypes.Breastplate));
            Trait bottomTrait = SelectTraitBottoms(tempTrait);
            if (bottomTrait != null) currentMetadata.Add(bottomTrait);
        }
        // ===============REMOVE WHEN NFTS HAVE BOOTS AND LEGGUARD TRAITS=====================================

        // select correct sigil shape
        if (currentMetadata.Exists(x => x.trait_type == nameof(TraitTypes.Sigil)))
        {
            Trait shield = currentMetadata.Find(x => x.trait_type == nameof(TraitTypes.Shield));
           if(shield.value.Contains("Circle")) shieldType = "CShield";
           else if(shield.value.Contains("Triangle")) shieldType = "TShield";
           else Debug.LogWarning($"Warning! Invalid Shield/Sigil combination for nft #{nftMetaData.token_id}");
        }


        foreach (Trait trait in currentMetadata)
        {
            string[] traitSplit = trait.value.Split();
            string skinName;
            
            if (trait.trait_type == nameof(TraitTypes.Sigil))
            {
                skinName = trait.trait_type + "_" + shieldType + "_" + string.Join('_', traitSplit);
            }
            else
            {
                skinName = trait.trait_type + "_" + string.Join('_', traitSplit);
            }

            Skin traitSkin = knightSkeletonData.Skins.Find(x =>
                x.Name.Contains(trait.trait_type + "_" + string.Join('_', traitSplit)));
            if (traitSkin == null)
            {
                Debug.LogWarning($"[PlayerSpriteManager] {trait.trait_type} Skin not found for {trait.value}");
                continue;
            }

            List<Skin.SkinEntry> skinEntries = traitSkin.Attachments.ToList();
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

    //====================TEMP CODE, DELETE WHEN NFTS HAVE BOOTS AND LEGGUARD TRAITS==============
    private Trait SelectTraitBottoms(Trait traitData)
    {
        if (traitData.trait_type == nameof(TraitTypes.Breastplate))
        {
            switch (traitData.value)
            {
                case "Gothic":
                case "Engraved":
                case "Galactic":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Gothic"
                    };
                case "Gold Gothic":
                case "Gold Engraved":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Gold Gothic"
                    };
                case "Bloodied Gothic":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Bloodied Gothic"
                    };
                case "Red Leather":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Red Leather"
                    };

                case "Blue Leather":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Blue Leather"
                    };
                case "Purple Leather":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Purple Leather"
                    };
                case "Red Dragon Rent":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Red Dragon Rent"
                    };
                case "Blue Dragon Rent":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Blue Dragon Rent"
                    };
                case "Purple Dragon Rent":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Purple Dragon Rent"
                    };
                case "Royal Guard":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Royal Guard"
                    };
                case "Blue and Gold":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Blue and Gold"
                    };

                case "White Hand":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "White Hand"
                    };

                case "Gold White Hand":
                case "Gold Templar":
                case "Gold Royal Guard":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Gold White Hand"
                    };
                case "Dark":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Dark"
                    };
                case "Ancient":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Ancient"
                    };
                case "Medici":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Legguard),
                        value = "Medici"
                    };
            }
        }

        if (traitData.trait_type == nameof(TraitTypes.Gauntlet))
        {
            switch (traitData.value)
            {
                case "Leather":
                case "Balthasar":
                case "Hourglass":
                case "Gold Hourglass":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Leather"
                    };
                case "Churburg Hourglass":
                case "Elven":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Churburg Hourglass"
                    };
                case "Gold Churburg Hourglass":
                case "Gold Knuckle":
                case "Gold Articulated":
                case "Gold Mitten":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Gold Churburg Hourglass"
                    };
                case "Mitten":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Mitten"
                    };
                case "Volcanic":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Volcanic"
                    };
                case "Spiked":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Spiked"
                    };
                case "Bloodied":
                case "Dread":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Dread"
                    };
                case "Gold Dread":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Gold Dread"
                    };
                case "Flaming":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Flaming"
                    };
                case "Blue Flaming":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Blue Flaming Dread"
                    };
                case "Articulated":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Articulated"
                    };
                case "Medici":
                    return new Trait
                    {
                        trait_type = nameof(TraitTypes.Boots),
                        value = "Medici"
                    };
            }
        }

        return null;
    }
}