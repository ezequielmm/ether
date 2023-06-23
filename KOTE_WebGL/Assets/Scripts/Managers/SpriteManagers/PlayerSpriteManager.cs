using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSpriteManager : SingleTon<PlayerSpriteManager>
{
    public SkeletonDataAsset KinghtData;
    public SpriteList DefaultSkinImages;
    public UnityEvent skinLoading { get; } = new();

    private PlayerNft _curNft;
    private List<PlayerNft> characterList = new List<PlayerNft>();
    private UnityAction nftLoadedListener;


    public UnityEvent OnSkinLoaded;
    
    private SkeletonData knightSkeletonData => KinghtData.GetSkeletonData(true);

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        UserDataManager.Instance.ExpeditionStatusUpdated.AddListener(ClearEquippedGearIfNoExpedition);
    }

    private void ClearEquippedGearIfNoExpedition()
    {
        if (UserDataManager.Instance.EquippedGear == null || UserDataManager.Instance.EquippedGear.Count == 0)
        {
            foreach (PlayerNft nft in characterList)
            {
                nft.ClearEquippedGear();
            }
        }
        if(_curNft != null) UpdatePlayerSkin();
    }

    public void SetSkin(int nftToken, NftContract contract, List<GearItemData> equippedGear = null)
    {
        if (nftToken < 0)
        {
            return;
        }

        if (NftManager.Instance.Nfts.Keys.Count == 0)
        {
            if(nftLoadedListener != null) NftManager.Instance.NftsLoaded.RemoveListener(nftLoadedListener);
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

        if (equippedGear == null)
        {
            if (curNft.TokenId == UserDataManager.Instance.ActiveNft && UserDataManager.Instance.EquippedGear != null)
            {
                EquipGearToStartingNft(curNft, UserDataManager.Instance.EquippedGear);
            }
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


    public void UpdateNftTrait(Trait trait, string traitValue)
    {
        if (trait == Trait.Padding)
        {
            _curNft.ChangeGear(Trait.Upper_Padding, traitValue);
            _curNft.ChangeGear(Trait.Lower_Padding, traitValue);
        }
        else
        {
            _curNft.ChangeGear(trait, traitValue);
        }
        UpdatePlayerSkin();
    }

    public void BuildPlayer(Nft selectedNft)
    {
        PlayerNft.ClearCache();

        _curNft = GetNftBasedOnMetadata(selectedNft);
        int curNftIndex = characterList.FindIndex(x => x == _curNft);
        if (curNftIndex != -1 && curNftIndex != characterList.Count - 1)
        {
            CacheSkin(characterList[curNftIndex + 1]);
        }
        
        UpdatePlayerSkin();
    }

    public async void CachePlayerSkinsAtStartup(List<Nft> tokens)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            PlayerNft curNft = GetNftBasedOnMetadata(tokens[i]);
            if (i < GameSettings.INITIAL_SKIN_CACHE)
            {
                await CacheSkin(curNft);
            }
        }
    }

    private async UniTask CacheSkin(PlayerNft skin)
    {
        await skin.GetDefaultSprites(knightSkeletonData);
        await skin.GetNftSprites(knightSkeletonData);
    }

    private async void UpdatePlayerSkin()
    {
        // TODO: Totally a Memory Leak
        skinLoading.Invoke();
        await _curNft.GetDefaultSprites(knightSkeletonData);
        await _curNft.GetNftSprites(knightSkeletonData);
        
        GameManager.Instance.UpdatePlayerSkin();
        OnSkinLoaded.Invoke();
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
            case NftContract.NonTokenVillager:
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