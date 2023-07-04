using System;
using System.Collections;
using System.Linq;
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

        ReplacePaddingTraitWithSplit();
    }

    public override void ChangeGear(Trait trait, string traitValue)
    {
        if (string.IsNullOrEmpty(traitValue))
        {
            EquippedTraits.Remove(trait);
            return;
        }

        EquippedTraits[trait] = traitValue;
    }

    public override IEnumerator GetNftSprites(SkeletonData playerSkeleton, Action callback)
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

    
}