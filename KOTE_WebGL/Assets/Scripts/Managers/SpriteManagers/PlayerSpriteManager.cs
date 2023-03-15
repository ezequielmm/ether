using System;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public SkeletonDataAsset KinghtData;
    public SpriteList DefaultSkinImages;
    private PlayerNft playerNft;

    private SkeletonData knightSkeletonData;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(BuildPlayer);

        knightSkeletonData = KinghtData.GetSkeletonData(true);
    }

    public async void SetSkin(int nftToken) 
    {
        if (nftToken < 0) { return; }
        List<Nft> nftList = await FetchData.Instance.GetNftMetaData(new List<int> { nftToken }, NftContract.KnightsOfTheEther);
        BuildPlayer(nftList[0]);
    }

    private async void BuildPlayer(Nft selectedNft)
    {
        this.playerNft = new PlayerNft(selectedNft);
        Debug.Log($"[PlayerSpriteManager] Nft #{selectedNft.TokenId} has been selected.");
        await playerNft.GetDefaultSprits(knightSkeletonData, DefaultSkinImages);
        await playerNft.GetNftSprites(knightSkeletonData);
        GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.Invoke();
    }

    public List<TraitSprite> GetAllTraitSprites()
    {
        return playerNft?.FullSpriteList() ?? new List<TraitSprite>();
    }
}