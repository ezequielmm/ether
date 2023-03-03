using Cysharp.Threading.Tasks;
using Spine;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerNft
{
    public Nft Metadata;
    public Dictionary<Trait, string> Traits => Metadata.Traits;

    public List<TraitSprite> SkinSprites = new List<TraitSprite>();
    public List<TraitSprite> DefaultSprites = new List<TraitSprite>();

    public PlayerNft(Nft nftData) 
    {
        Metadata = nftData;
        InjectTempTraits();
    }

    public async UniTask GetNftSprites(SkeletonData playerSkeleton) 
    {
        if(Metadata == null) 
        {
            throw new NullReferenceException("Nft Metadata cannot be null.");
        }
        foreach (Trait trait in Traits.Keys) 
        {
            string traitValue = Traits[trait];
            string skinName = GetSkinName(trait, traitValue);
            Skin traitSkin = playerSkeleton.Skins.Find(x => x.Name.Contains(skinName));
            if (traitSkin == null)
            {
                Debug.LogWarning($"[PlayerNft] {trait} skin not found for {traitValue}");
                continue;
            }

            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                TraitSprite spriteData = GenerateSpriteData(skinEntry, traitSkin.Name, traitValue);
                Sprite skinElement = await GetPlayerSkin(spriteData);
                spriteData.Sprite = skinElement;
                SkinSprites.Add(spriteData);
            }
        }
    }

    private string FormatImageName(Skin.SkinEntry skinEntry) 
    {
        string[] baseImageName = skinEntry.Attachment.Name.Split('/');
        string imageName = (baseImageName.Length > 1) ? baseImageName[1] : baseImageName[0];
        imageName = imageName.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        if (imageName.Contains("placeholder")) return null;

        return imageName;
    }

    public async UniTask GetDefaultSprits(SkeletonData playerSkeleton) 
    {
        for (int i = 0; i < GameSettings.DEFAULT_SKIN_DATA.Length; i++)
        {
            TraitSprite spriteData = GameSettings.DEFAULT_SKIN_DATA[i];
            Skin traitSkin = playerSkeleton.Skins.Find(x => x.Name == spriteData.SkinName);
            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                string imageName = FormatImageName(skinEntry);
                if (string.IsNullOrEmpty(imageName)) continue;

                spriteData.AttachmentIndex = skinEntry.SlotIndex;
                spriteData.ImageName = imageName;

                Sprite skinElement = await GetPlayerSkin(spriteData);
                spriteData.Sprite = skinElement;
                DefaultSprites.Add(spriteData);
            }
        }
    }

    private TraitSprite GenerateSpriteData(Skin.SkinEntry skinEntry, string skinName, string traitValue) 
    {
        string imageName = FormatImageName(skinEntry);
        if(string.IsNullOrEmpty(imageName)) return null;
        TraitSprite spriteData = new TraitSprite
        {
            SkinName = skinName,
            TraitType = traitValue,
            AttachmentIndex = skinEntry.SlotIndex,
            ImageName = imageName
        };

        return spriteData;
    }

    private async UniTask<Sprite> GetPlayerSkin(TraitSprite spriteData) 
    {
        Texture2D texture = await FetchData.Instance.GetNftSkinElement(spriteData);
        return texture.ToSprite();
    }

    private string GetSkinName(Trait trait, string value)
    {
        string skinName = value;
        skinName = skinName.Replace('-', '_').Replace(' ', '_');

        if (trait == Trait.Sigil)
        {
            skinName = $"{GetShieldType()}_{skinName}";
        }

        skinName = $"{trait}_{skinName}";
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
        Debug.LogWarning($"Warning! Invalid Shield/Sigil combination for nft #{Metadata.TokenId}");
        return string.Empty;
    }

    #region Temp Code
    // Temp Code as Boots and Legguards don't have traits in the Nft.
    // It'll disable itself if it does have the trait though, so it's future proof.
    private void InjectTempTraits() 
    {
        if (Traits.ContainsKey(Trait.Gauntlet) && !Traits.ContainsKey(Trait.Boots)) 
        {
            string bottomTrait = SelectTraitPair(Trait.Gauntlet, Traits[Trait.Gauntlet]);
            if (!string.IsNullOrEmpty(bottomTrait)) 
            {
                Traits.Add(Trait.Boots, bottomTrait);
            }
        }
        if (Traits.ContainsKey(Trait.Breastplate) && !Traits.ContainsKey(Trait.Legguard))
        {
            string bottomTrait = SelectTraitPair(Trait.Breastplate, Traits[Trait.Breastplate]);
            if (!string.IsNullOrEmpty(bottomTrait))
            {
                Traits.Add(Trait.Legguard, bottomTrait);
            }
        }
    }
    private string SelectTraitPair(Trait trait, string value)
    {
        if (trait == Trait.Breastplate)
        {
            switch (value)
            {
                case "Engraved":
                case "Galactic":
                    return "Gothic";
                case "Gold Engraved":
                    return "Gold Gothic";
                case "Gold Templar":
                case "Gold Royal Guard":
                    return "Gold White Hand";
                default: 
                    return value;
            }
        }

        if (trait == Trait.Gauntlet)
        {
            switch (value)
            {
                case "Balthasar":
                case "Hourglass":
                case "Gold Hourglass":
                    return "Leather";
                case "Elven":
                    return "Churburg Hourglass";
                case "Gold Knuckle":
                case "Gold Articulated":
                case "Gold Mitten":
                    return "Gold Churburg Hourglass";
                case "Bloodied":
                    return "Dread";
                default: 
                    return value;
            }
        }

        return null;
    }
    #endregion
}


[Serializable]
public class TraitSprite
{
    public string SkinName;
    public string TraitType;
    public int AttachmentIndex;
    public string ImageName;
    public Sprite Sprite;
}