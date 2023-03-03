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
    private PlayerNft playerNft;

    private SkeletonData knightSkeletonData;

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(BuildPlayer);
        knightSkeletonData = KinghtData.GetSkeletonData(true);
    }

    private async void BuildPlayer(Nft selectedNft)
    {
        this.playerNft = new PlayerNft(selectedNft);
        Debug.Log($"[PlayerSpriteManager] Nft #{selectedNft.TokenId} has been selected.");
        await playerNft.GetDefaultSprits(knightSkeletonData);
        await playerNft.GetNftSprites(knightSkeletonData);
        GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.Invoke();
    }

    public List<TraitSprite> GetAllTraitSprites()
    {
        List<TraitSprite> allSprites = new List<TraitSprite>();
        foreach (var traitType in Enum.GetNames(typeof(Trait)))
        {
            if (playerNft.SkinSprites.Exists(x => x.TraitType == traitType))
            {
                allSprites.AddRange(playerNft.SkinSprites.FindAll(x => x.TraitType == traitType));
                continue;
            }
            if (traitType == nameof(Trait.Sigil) || traitType == nameof(Trait.Crest)
                                                      || traitType == nameof(Trait.Vambrace))
            {
                continue;
            }

            if (playerNft.DefaultSprites.Exists(x => x.TraitType == traitType))
            {
                allSprites.AddRange(playerNft.DefaultSprites.FindAll(x => x.TraitType == traitType));
            }
        }
        return allSprites;
    }
}