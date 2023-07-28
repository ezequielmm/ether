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

    public void SetContractAddress(NftContract contract, string address)
    {
        if (IsTestNet)
        {
            testNetNftContractMap[contract] = address;
            return;
        }

        etheriumNftContractMap[contract] = address;
    }

    public string GetContractAddress(NftContract contract)
    {
        if (IsTestNet) return testNetNftContractMap[contract];
        return etheriumNftContractMap[contract];
    }

    private void UpdateNfts(RawWalletData walletData)
    {
        Nfts.Clear();
        Nfts[NftContract.NonTokenVillager] = new List<Nft> { GameSettings.DEFAULT_PLAYER };
        foreach (ContractData contract in walletData.Contracts)
        {
            if (contract.ContractType == NftContract.None) continue;
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
                if (token.can_play == false)
                {
                    Debug.Log("Token cant play");
                }
                if (token.metadata.CanPlay == false)
                {
                    Debug.Log("Token cant " +
                        "play 2 ");
                }
                Nfts[contract.ContractType].Add(token.metadata);
            }
        }

        NftsLoaded.Invoke();
    }

    private static Dictionary<NftContract, string> etheriumNftContractMap = new()
    {
        { NftContract.Knights, "0x32A322C7C77840c383961B8aB503c9f45440c81f" },
        { NftContract.Villager, "0xbFfd759b9F7d07ac76797cc13974031Eb23e5757" },
        { NftContract.BlessedVillager, "0x2d51402A6DAb0EA48E30Bb169db74FfE3c1c6675" },
        { NftContract.NonTokenVillager, "" }
    };

    private static Dictionary<NftContract, string> testNetNftContractMap = new()
    {
        { NftContract.Knights, "0x80e2109a826148b9b1a41b0958ca53a4cdc64b70" },
        { NftContract.Villager, "0xbFfd759b9F7d07ac76797cc13974031Eb23e5757" },
        { NftContract.BlessedVillager, "0x55abb816b145CA8F34ffA22D63fBC5bc57186690" },
        { NftContract.NonTokenVillager, "" }
    };

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