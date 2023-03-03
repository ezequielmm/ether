using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class NftManager : ISingleton<NftManager>
{
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

    private NftManager()
    {
        wallet = WalletManager.Instance;
        wallet.NewWalletConfirmed.AddListener(UpdateNfts);
    }

    public List<Nft> GetContractNfts(NftContract nftContract) 
    {
        return Nfts[nftContract] ?? new List<Nft>();
    }
    public List<Nft> GetAllNfts()
    {
        List<Nft> returnList = new List<Nft>();
        foreach (NftContract contract in Nfts.Keys)
        {
            returnList.AddRange(GetContractNfts(contract));
        }
        return returnList;
    }

    private async void UpdateNfts(string WalletAddress) 
    {
        var NftTokenMap = WalletManager.Instance.NftsInWallet;
        foreach (NftContract contract in NftTokenMap.Keys) 
        {
            List<Nft> nfts = await FetchData.Instance.GetNftMetaData(NftTokenMap[contract], contract);
            Nfts.Add(contract, nfts);
        }
        NftsLoaded.Invoke();
    }

    private static readonly Dictionary<NftContract, string> nftContractMap = new() {
        { NftContract.KnightsOfTheEther, "0x32A322C7C77840c383961B8aB503c9f45440c81f" }
    };
    public static string GetNftContractAddress(NftContract contract)
    {
        return nftContractMap[contract];
    }
}
public enum NftContract
{
    KnightsOfTheEther
}

