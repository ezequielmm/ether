using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NftManager : ISingleton<NftManager>
{
    public static readonly bool IsTestNet = true;
    private static NftManager instance;

    public static NftManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NftManager();
            }

            return instance;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }

    public UnityEvent NftsLoaded { get; } = new();

    private WalletManager wallet;
    public Dictionary<NftContract, List<Nft>> Nfts = new();

    private Nft nftSelected;
    public Nft NftSelected => nftSelected;
    
    private NftManager()
    {
        wallet = WalletManager.Instance;
        wallet.NewWalletConfirmed.AddListener(UpdateNfts);
    }

    public List<Nft> GetContractNfts(NftContract nftContract)
    {
        if (Nfts.TryGetValue(nftContract, out List<Nft> nfts) && nfts != null)
        {
            return nfts;
        }

        Nfts.Add(nftContract, new List<Nft>());
        return Nfts[nftContract];
    }

    public List<Nft> GetAllNfts()
    {
        List<Nft> returnList = new List<Nft>();
        // make sure the lists are always return in the order from the enum
        foreach (NftContract contract in Enum.GetValues(typeof(NftContract)))
        {
            if (contract == NftContract.None || !Nfts.ContainsKey(contract)) continue;
            returnList.AddRange(GetContractNfts(contract));
        }

        return returnList;
    }
    
    private void UpdateNfts(RawWalletData walletData)
    {
        Nfts.Clear();
        Nfts[NftContract.NonTokenVillager] = new List<Nft> { GameSettings.DEFAULT_PLAYER };
        foreach (ContractData contract in walletData.Contracts)
        {
            if (contract.ContractType == NftContract.None) continue;
            if (!Nfts.ContainsKey(contract.ContractType))
                Nfts[contract.ContractType] = new List<Nft>();
            foreach (TokenData token in contract.tokens)
            {
                // if the metadata is null then the backend detected bad data and didn't send anything
                if (token.metadata == null)
                {
                    Debug.LogError(
                        $"[NftManager] No Metadata Found for the {contract.ContractType} {token.token_id} From The Server");
                    continue;
                }
                Debug.Log("Token Can Play: " + token.can_play);
                token.metadata.TokenId = int.Parse(token.token_id);
                token.metadata.Contract = contract.ContractType;
                token.metadata.adaptedImageURI = token.adaptedImageURI;
                token.metadata.CanPlay = token.can_play;

                var isInitiated = contract.characterClass.Contains("initiated");
                token.metadata.IsInitiated = isInitiated;
                token.metadata.ContractAddress = contract.contract_address;

                if (isInitiated)
                    RequestRealGear(token);
                    
                Nfts[contract.ContractType].Add(token.metadata);
            }
        }

        NftsLoaded.Invoke();
    }

    private void RequestRealGear(TokenData token)
    {
        MonoBehaviour.FindObjectOfType<MonoBehaviour>().StartCoroutine(RequestService.Instance.GetRequestCoroutine(
            $"{ClientEnvironmentManager.Instance.InitiatedGearUrl}{token.token_id}/{token.metadata.ContractAddress}",
            data =>
            {
                token.metadata.initiatedRealItems = FetchData.ParseJsonWithPath<List<VictoryItems>>(data, "realItems");
            }));
    }

    public void SetNft(Nft nft)
    {
        nftSelected = nft;
    }
}

// these are named as such to match backend
public enum NftContract
{
    None, // default so we can have a bad contract check
    NonTokenVillager,
    Knights,
    BlessedVillager,
    Villager
}