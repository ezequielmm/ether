using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public SkeletonDataAsset KinghtData;
    public SpriteList DefaultSkinImages;

    private PlayerNft _curNft;
    private List<PlayerNft> characterList = new List<PlayerNft>();


    private SkeletonData knightSkeletonData => KinghtData.GetSkeletonData(true);

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(BuildPlayer);
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(UpdateNftForExpedition);
        GameManager.Instance.EVENT_UPDATE_NFT.AddListener(UpdateNftTrait);
    }

    private void UpdateNftForExpedition(bool data, int nftInt)
    {
        SetSkin(nftInt);
    }

    public async void SetSkin(int nftToken)
    {
        if (nftToken < 0)
        {
            return;
        }

        List<Nft> nftList =
            await FetchData.Instance.GetNftMetaData(new List<int> { nftToken }, NftContract.KnightsOfTheEther);
        BuildPlayer(nftList[0]);
    }

    private void UpdateNftTrait(Trait trait, string traitValue)
    {
        _curNft.ChangeGear(trait, traitValue);
        UpdatePlayerSkin();
    }

    private  void BuildPlayer(Nft selectedNft)
    {
        _curNft = GetNftBasedOnMetadata(selectedNft);
        Debug.Log($"[PlayerSpriteManager] Nft #{selectedNft.TokenId} has been selected.");
        UpdatePlayerSkin();
    }

    private async void UpdatePlayerSkin()
    {
        await _curNft.GetDefaultSprites(knightSkeletonData);
        await _curNft.GetNftSprites(knightSkeletonData);
        GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.Invoke();
    }

    private PlayerNft GetNftBasedOnMetadata(Nft selectedNft)
    {
        // we do the find this way in case we have a different instance of the metadata being sent in
        PlayerNft foundNft = characterList.Find(x =>
            x.Metadata.Contract == selectedNft.Contract && x.Metadata.TokenId == selectedNft.TokenId);
        if (foundNft == null)
        {
            foundNft = CreateNftInstance(selectedNft);
        }

        return foundNft;
    }

    private PlayerNft CreateNftInstance(Nft metadata)
    {
        switch (metadata.Contract)
        {
            case NftContract.KnightsOfTheEther:
                return new Knight(metadata);
            case NftContract.Villager:
            case NftContract.BlessedVillager:
                return new Villager(metadata);
            default:
                return null;
        }
    }

    public List<TraitSprite> GetAllTraitSprites()
    {
        return _curNft?.FullSpriteList() ?? new List<TraitSprite>();
    }
}