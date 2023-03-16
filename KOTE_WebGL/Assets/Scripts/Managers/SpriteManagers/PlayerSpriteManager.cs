using System.Collections.Generic;
using KOTE.UI.Armory;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public SkeletonDataAsset KinghtData;
    public SpriteList DefaultSkinImages;

    private PlayerNft _curNft;
    private List<PlayerNft> characterList = new List<PlayerNft>();
    private UnityAction nftLoadedListener;


    private SkeletonData knightSkeletonData => KinghtData.GetSkeletonData(true);

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        GameManager.Instance.EVENT_NFT_SELECTED.AddListener(BuildPlayer);
    }

    public void SetSkin(int nftToken, NftContract contract, List<GearItemData> equippedGear = null)
    {
        if (nftToken < 0)
        {
            return;
        }

        bool nftLoaded = false;
        if (NftManager.Instance.Nfts.Keys.Count == 0)
        {
            nftLoadedListener = () => { WaitForNftLoad(nftToken, contract, equippedGear); };
            NftManager.Instance.NftsLoaded.AddListener(nftLoadedListener);
            return;
        }

        Nft curNft = null;

        if (NftManager.Instance.GetContractNfts(contract).Exists(x => x.TokenId == nftToken))
        {
            curNft = NftManager.Instance.GetContractNfts(contract).Find(x => x.TokenId == nftToken);
        }

        if (curNft == null)
        {
            Debug.LogWarning($"Loaded NFT skin {nftToken} not owned by player");
            return;
        }

        if (equippedGear != null)
        {
            EquipGearToStartingNft(curNft, equippedGear);
        }

        BuildPlayer(curNft);
    }

    private void EquipGearToStartingNft(Nft metadata, List<GearItemData> equippedGear)
    {
        PlayerNft curNft = GetNftBasedOnMetadata(metadata);
        foreach (GearItemData itemData in equippedGear)
        {
            curNft.ChangeGear(itemData.trait.ParseToEnum<Trait>(), itemData.name);
        }
    }


    private void WaitForNftLoad(int nftToken, NftContract contract, List<GearItemData> equippedGear)
    {
        NftManager.Instance.NftsLoaded.RemoveListener(nftLoadedListener);
        nftLoadedListener = null;
        SetSkin(nftToken, contract, equippedGear);
    }


    private void UpdateNftTrait(Trait trait, string traitValue)
    {
        _curNft.ChangeGear(trait, traitValue);
        UpdatePlayerSkin();
    }

    private void BuildPlayer(Nft selectedNft)
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
            characterList.Add(foundNft);
        }

        return foundNft;
    }

    private PlayerNft CreateNftInstance(Nft metadata)
    {
        switch (metadata.Contract)
        {
            case NftContract.Knights:
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