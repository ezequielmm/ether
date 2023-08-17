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
}