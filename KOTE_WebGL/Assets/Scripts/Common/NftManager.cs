using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NftManager : ISingleton<NftManager>
{
    private static NftManager instance;
    public static NftManager Instance 
    {
        get 
        {
            if(instance == null) 
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

    public string ActiveWallet { get; private set; }
    //public Dictionary<NftContract, List<NftData>>;


    private NftManager() 
    { }


    public void RequestActiveWallet() 
    {
        
    }

    public void SelectedAccountChanged(string newAccount) 
    {
    
    }

    public void ForgetWallet(string walletToRemove) 
    {
        if(walletToRemove == ActiveWallet) { RemoveActiveWallet(); }
    }

    public void RemoveActiveWallet() 
    {
    
    }

    public int GetNftCountForContract(NftContract contract) 
    {
        return -1;
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