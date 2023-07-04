using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Spine;
using UnityEngine;

[Serializable]
public class Knight : PlayerNft
{
    public Knight(Nft nftData)
    {
        Metadata = nftData;
        Traits = Metadata.Traits;
        ReplacePaddingTraitWithSplit();
        InjectTempTraits();
    }

    public override void ChangeGear(Trait trait, string traitValue)
    {
        // knights cannot change gear
    }


    public override IEnumerator GetNftSprites(SkeletonData playerSkeleton, Action callback)
    {
        if (Metadata == null)
        {
            throw new NullReferenceException("Nft Metadata cannot be null.");
        }

        var pending = 0;
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
                TraitSprite spriteData = GenerateSpriteData(skinEntry, traitSkin.Name, traitValue, trait);
                if (spriteData == null)
                {
                    // ignore placeholder
                    continue;
                }

                // if (DoesSkinSpriteExist(spriteData))
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
    public string TraitValue;
    public Trait TraitType;
    public int AttachmentIndex;
    public string ImageName;
    public Sprite Sprite;

    public bool IsUseableInSkin => Sprite != null && !string.IsNullOrEmpty(TraitValue)
                                                  && !string.IsNullOrEmpty(SkinName);

    public override string ToString()
    {
        return
            $"Skin: {SkinName} | Trait: {TraitValue} | Attachment: {AttachmentIndex} | Image: {ImageName} | Sprite Set? {Sprite != null}";
    }
}