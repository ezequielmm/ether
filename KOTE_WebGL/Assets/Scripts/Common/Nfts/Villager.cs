using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spine;
using UnityEngine;

public class Villager : PlayerNft
{
    public Villager(Nft nftData)
    {
        Metadata = nftData;
        BuildBaseTraits();
    }

    private void BuildBaseTraits()
    {
        foreach (var defaultTrait in GameSettings.DEFAULT_SKIN_DATA)
        {
            Traits[defaultTrait.TraitType] = defaultTrait.TraitValue;
        }

        foreach (var trait in Metadata.Traits)
        {
            Traits[trait.Key] = trait.Value;
        }
    }

    public override void ChangeGear(Trait trait, string traitValue)
    {
        EquippedTraits[trait] = traitValue;
    }

    public override async UniTask GetNftSprites(SkeletonData playerSkeleton)
    {
        foreach (Trait trait in Traits.Keys)
        {
            string traitValue = (EquippedTraits.ContainsKey(trait)) ? EquippedTraits[trait] : Traits[trait];

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

            foreach (Skin.SkinEntry skinEntry in traitSkin.Attachments)
            {
                TraitSprite spriteData = GenerateSpriteData(skinEntry, traitSkin.Name, traitValue, trait);
                if (spriteData == null)
                {
                    // ignore placeholder
                    continue;
                }

                if (SkinSprites.Find(x => x.ImageName == spriteData.ImageName) != null || 
                    DefaultSprites.Find(x => x.ImageName == spriteData.ImageName) != null)
                {
                    // Sprite already fetched
                    continue;
                }

                Sprite skinElement = await GetPlayerSkin(spriteData);
                spriteData.Sprite = skinElement;
                if (!spriteData.IsUseableInSkin)
                {
                    Debug.LogError($"[PlayerNft] Can not use current TraitSprite. {spriteData}");
                    continue;
                }

                SkinSprites.Add(spriteData);
            }
        }
    }
}