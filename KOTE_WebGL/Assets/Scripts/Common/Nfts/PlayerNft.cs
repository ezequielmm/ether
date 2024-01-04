using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Spine;
using UnityEngine;

public abstract class PlayerNft
{
    // made static so that the stored sprite data persists regardless of character type
    public static List<TraitSprite> SkinSprites = new List<TraitSprite>();
    public static List<TraitSprite> DefaultSprites = new List<TraitSprite>();

    public Nft Metadata;

    protected Dictionary<Trait, string> Traits = new();
    protected Dictionary<Trait, string> EquippedTraits = new();

    public virtual IEnumerator GetNftSprites(SkeletonData playerSkeleton, Action callback)
    {
        var pending = 0;
        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            string traitValue;

            if (EquippedTraits.ContainsKey(trait))
            {
                traitValue = EquippedTraits[trait];
            }
            else if (Traits.ContainsKey(trait))
            {
                traitValue = Traits[trait];
            }
            else
            {
                continue;
            }

            if (traitValue.Equals("None"))
            {
                // don't worry about processing this since there's no skin
                continue;
            }

            string skinName;

            if (trait != Trait.Base && trait != Trait.Shadow)
            {
                skinName = GetSkinName(trait, traitValue);
            }
            else
            {
                skinName = GameSettings.DEFAULT_SKIN_DATA.Find(x => x.TraitType == trait).SkinName;
            }
            
            Skin traitSkin = playerSkeleton.Skins.Find(x => x.Name.Contains(skinName));
            if (traitSkin == null)
            {
                Debug.LogWarning($"[PlayerNft] {trait} skin not found for {traitValue}");
                continue;
            }

            var list = traitSkin.Attachments.ToArray();
            foreach (Skin.SkinEntry skinEntry in list)
            {
                TraitSprite spriteData = GenerateSpriteData(skinEntry, traitSkin.Name, traitValue, trait);
                if (spriteData == null)
                {
                    // ignore placeholder
                    continue;
                }

                // if (DoesSkinSpriteExist(spriteData) || DoesDefaultSpriteExist(spriteData))
                // {
                //     // Sprite already fetched
                //     continue;
                // }

                pending++;
                GetPlayerSkin(spriteData, skinElement =>
                {
                    pending--;
                    spriteData.Sprite = skinElement;
                    if (!spriteData.IsUseableInSkin)
                        Debug.LogWarning($"[PlayerNft] Can not use current TraitSprite. {spriteData}");
                    else
                        SkinSprites.Add(spriteData);
                });
            }
        }

        while (pending > 0)
            yield return null;

        callback?.Invoke();
    }

    public virtual void ChangeGear(Trait trait, string traitValue)
    {
        if (string.IsNullOrEmpty(traitValue))
        {
            EquippedTraits.Remove(trait);
            return;
        }

        EquippedTraits[trait] = traitValue;
    }

    public void ClearEquippedGear()
    {
        EquippedTraits.Clear();
    }

    public IEnumerator GetDefaultSprites(SkeletonData playerSkeleton, Action callback)
    {
        var pending = 0;
        for (int i = 0; i < GameSettings.DEFAULT_SKIN_DATA.Count; i++)
        {
            TraitSprite spriteData = GameSettings.DEFAULT_SKIN_DATA[i];
            Skin traitSkin = playerSkeleton.Skins.Find(x => x.Name == spriteData.SkinName);
            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                // clone the TraitSprite so we get an entry for each sprite of a skin
                TraitSprite actualSpriteData = new TraitSprite
                {
                    AttachmentIndex = spriteData.AttachmentIndex,
                    ImageName = spriteData.ImageName,
                    SkinName = spriteData.SkinName,
                    Sprite = spriteData.Sprite,
                    TraitType = spriteData.TraitType,
                    TraitValue = spriteData.TraitValue
                };
                string imageName = FormatImageName(skinEntry);
                if (string.IsNullOrEmpty(imageName)) continue;

                actualSpriteData.AttachmentIndex = skinEntry.SlotIndex;
                actualSpriteData.ImageName = imageName;

                if (DefaultSprites.Find(x =>
                        x.ImageName == actualSpriteData.ImageName && !x.SkinName.Contains("nude")) != null)
                {
                    // Sprite already fetched
                    continue;
                }
                
                Action<Sprite> inner = (skinElement) =>
                {
                    pending--;
                    actualSpriteData.Sprite = skinElement;
                    if (!actualSpriteData.IsUseableInSkin)
                        Debug.LogWarning($"[PlayerNft] Can not use current TraitSprite. {actualSpriteData}");
                    else
                        DefaultSprites.Add(actualSpriteData);
                };

                Sprite skinElement =
                    PlayerSpriteManager.Instance.DefaultSkinImages.entityImages.Find(s => s.name == imageName);
                
                pending++;
                if (skinElement == null) {
                    GetPlayerSkin(actualSpriteData, inner);
                }
                else {
                    inner(skinElement);
                }
            }
        }

        while (pending > 0)
            yield return null;

        callback?.Invoke();
    }

    public List<TraitSprite> FullSpriteList()
    {
        List<TraitSprite> allSprites = new List<TraitSprite>();
        foreach (Trait trait in Enum.GetValues(typeof(Trait)))
        {
            if (EquippedTraits.ContainsKey(trait) &&
                SkinSprites.Exists(x => x.TraitValue == EquippedTraits[trait] && x.TraitType == trait))
            {
                allSprites.AddRange(SkinSprites.FindAll(x =>
                    x.TraitValue == EquippedTraits[trait] && x.TraitType == trait));
            }

            else if (Traits.ContainsKey(trait) &&
                     SkinSprites.Exists(x => x.TraitValue == Traits[trait] && x.TraitType == trait))
            {
                allSprites.AddRange(SkinSprites.FindAll(x => x.TraitValue == Traits[trait] && x.TraitType == trait));
            }

            else if (DefaultSprites.Exists(x => x.TraitType == trait && x.TraitType == trait))
            {
                allSprites.AddRange(DefaultSprites.FindAll(x => x.TraitType == trait && x.TraitType == trait));
            }
        }

        return allSprites;
    }

    protected void ReplacePaddingTraitWithSplit()
    {
        if (!Traits.ContainsKey(Trait.Padding)) return;
        string paddingColor = Traits[Trait.Padding];
        Traits.Remove(Trait.Padding);
        Traits[Trait.Upper_Padding] = paddingColor;
        Traits[Trait.Lower_Padding] = paddingColor;
    }

    protected void GetPlayerSkin(TraitSprite spriteData, Action<Sprite> callback)
    {
        FetchData.Instance.GetNftSkinElement(spriteData, texture =>
        {
            if (texture == null) {
                Debug.LogWarning($"Skin image not found on server for {spriteData.ImageName}");
            }
            callback( texture ? texture.ToSprite() : null);
        });
        
    }

    protected TraitSprite GenerateSpriteData(Skin.SkinEntry skinEntry, string skinName, string traitValue,
        Trait traitType)
    {
        string imageName = FormatImageName(skinEntry);
        if (string.IsNullOrEmpty(imageName)) return null;
        TraitSprite spriteData = new TraitSprite
        {
            SkinName = skinName,
            TraitType = traitType,
            TraitValue = traitValue,
            AttachmentIndex = skinEntry.SlotIndex,
            ImageName = imageName
        };

        return spriteData;
    }

    protected string FormatImageName(Skin.SkinEntry skinEntry)
    {
        string imagePath = "";
        if (skinEntry.Attachment.GetType() == typeof(MeshAttachment))
        {
            imagePath = ((MeshAttachment)skinEntry.Attachment).Path;
        }
        else if (skinEntry.Attachment.GetType() == typeof(RegionAttachment))
        {
            imagePath = ((RegionAttachment)skinEntry.Attachment).Path;
        }

        if (string.IsNullOrEmpty(imagePath) || (!imagePath.Contains("undergarment") && !imagePath.Contains("FX")))
        {
            imagePath = skinEntry.Attachment.Name;
        }

        string[] baseImageName = imagePath.Split('/');
        if (baseImageName.Length > 2 && baseImageName[2].Contains("FX")) return baseImageName[2];
        string imageName = (baseImageName.Length > 1) ? baseImageName[1] : baseImageName[0];
        if (!imageName.Contains("TortoiseShield_straps"))
            imageName = imageName.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        if (imageName.Contains("placeholder")) return null;

        return imageName;
    }

    protected bool DoesSkinSpriteExist(TraitSprite spriteData)
    {
        TraitSprite skinSprite = SkinSprites.Find(x => x.ImageName == spriteData.ImageName);
        
        if (skinSprite == null)
        {
            return false;
        }

        return skinSprite.AttachmentIndex == spriteData.AttachmentIndex;
    }

    protected bool DoesDefaultSpriteExist(TraitSprite spriteData)
    {
        TraitSprite defaultSprite = SkinSprites.Find(x => x.ImageName == spriteData.ImageName);
        if (defaultSprite == null)
        {
            return false;
        }

        return defaultSprite.AttachmentIndex == spriteData.AttachmentIndex;
    }

    protected string GetSkinName(Trait trait, string value)
    {
        string skinName = value;
        skinName = skinName.Replace('-', '_').Replace(' ', '_').Replace("/", "");

        if (trait == Trait.Sigil)
        {
            skinName = $"{GetShieldType()}_{skinName}";
        }

        // have to handle bucket helmet different, as it doesn't match the other helmet names
        if (trait == Trait.Helmet && value.Contains("Bucket"))
        {
            skinName = "Bucket";
        }

        skinName = (trait == Trait.Crest) ? $"{trait}s_{skinName}" : $"{trait}_{skinName}";
        return skinName;
    }


    private string GetShieldType()
    {
        if (!Traits.ContainsKey(Trait.Sigil) || !Traits.ContainsKey(Trait.Shield))
        {
            return string.Empty;
        }

        string shieldName = Traits[Trait.Shield];
        if (shieldName.Contains("Circle"))
            return "CShield";
        if (shieldName.Contains("Triangle"))
            return "TShield";
        Debug.LogWarning($"Warning! Invalid Shield/Sigil combination for nft");
        return string.Empty;
    }

    public static void ClearCache()
    {
        foreach (var ts in SkinSprites)
        {
            MonoBehaviour.Destroy(ts.Sprite);
        }
        SkinSprites.Clear();
        SkinSprites = new List<TraitSprite>();
        DefaultSprites.Clear();
        DefaultSprites = new List<TraitSprite>();
    }
    
    public string[] GetActiveSkinNames
    {
        get
        {
            var skinNames = EquippedTraits.
                ToDictionary(
                    trait => trait.Key, 
                    trait => GetSkinName(trait.Key, trait.Value));

            foreach (var trait in Traits.Where(trait => !skinNames.ContainsKey(trait.Key)))
                skinNames.Add(trait.Key, GetSkinName(trait.Key, trait.Value));

            return skinNames.Values.ToArray();
        }
    }
}