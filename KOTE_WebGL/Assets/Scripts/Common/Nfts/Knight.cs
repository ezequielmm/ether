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


    public override IEnumerator GetNftSprites(SkeletonData playerSkeleton, Action callback)
    {
        if (Metadata == null)
        {
            throw new NullReferenceException("Nft Metadata cannot be null.");
        }

        yield return base.GetNftSprites(playerSkeleton, callback);
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