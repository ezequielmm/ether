using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class WalletManager : ISingleton<WalletManager>
{
    private static WalletManager instance;

    public static WalletManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WalletManager();
            }

            return instance;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }
    
    public UnityEvent<string> DisconnectingWallet { get; } = new();
    public UnityEvent WalletStatusModified { get; } = new();

    public string ActiveWallet => AuthenticationManager.LoginData.Wallet;
    public bool WalletVerified { get; set; } = false;
    public Dictionary<NftContract, List<int>> NftsInWallet = new();
    public const string INITIATED_SUCCESS_MESSAGE = "initiated-success";


    private WalletManager()
    {
    }

    public async void SetActiveWallet()
    {
        await ConnectWallet();
        WalletStatusModified.Invoke();
        if (string.IsNullOrEmpty(ActiveWallet))
        {
            Debug.LogWarning("[WalletManager] no active wallet received!");
            return;
        }
        bool ownWallet = await ConfirmActiveWalletOwnership();
        WalletStatusModified.Invoke();
        if (!ownWallet)
        {
            Debug.Log($"[WalletManager] Player does not own wallet");
            return;
        }

        RawWalletData walletData = await GetNftsInWallet(ActiveWallet);
        
        WalletStatusModified.Invoke();
        if (walletData == null)
        {
            Debug.LogWarning("No Wallet Contents retrieved");
        }

        WalletVerified = true;
        
        GameManager.Instance.NewWalletConfirmed(walletData);
        WalletStatusModified.Invoke();
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async UniTask ConnectWallet()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        SetWallet(AuthenticationManager.LoginData.Wallet);
      
    }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async UniTask<bool> ConfirmActiveWalletOwnership()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        return true;
    }

    public bool ConfirmNftOwnership(int nftId, NftContract tokenType)
    {
        //TODO: WTF
        return true;
        /*
        if (tokenType == NftContract.None)
        {
            Debug.Log($"[WalletManager] Player will never own a [{NftContract.None}] token type.");
        }

        // default non-contest token
        if (tokenType == NftContract.NonTokenVillager)
        {
            return true;
        }

        if (NftsInWallet.ContainsKey(tokenType))
        {
            return NftsInWallet[tokenType].Contains(nftId);
        }
        return false;*/
    }

    public bool ConfirmOwnsNfts()
    {
        foreach (NftContract contract in NftsInWallet.Keys)
        {
            if (NftsInWallet[contract] != null && NftsInWallet[contract].Count > 0) return true;
        }

        return false;
    }

    public void SelectedAccountChanged(string newAccount)
    {
        SetWallet(newAccount);
    }

    public void ForgetWallet(string walletToRemove)
    {
        if (walletToRemove == ActiveWallet)
        {
            RemoveActiveWallet();
        }
    }

    public void RemoveActiveWallet()
    {
        DisconnectingWallet.Invoke(ActiveWallet);
        SetWallet(null);
    }

    private void SetWallet(string newAddress)
    {

        WalletVerified = true;
        NftsInWallet.Clear();
    }

    public async UniTask<RawWalletData> GetNftsInWallet(string walletAddress)
    {
        Debug.Log($"[WalletManager] Fetching Wallet Contents...");
        RawWalletData nftData = await FetchData.Instance.GetNftsInWallet(walletAddress);
        if (nftData == null)
        {
            Debug.LogWarning("No Wallet Contents Received");
        }
        Debug.Log($"[WalletManager] Wallet Contents Received.");
        foreach (ContractData contractData in nftData.Contracts)
        {
            if (contractData.tokens == null || contractData.tokens.Count == 0) continue;
            NftContract contract = contractData.ContractType;
            
            if (!NftsInWallet.ContainsKey(contract))
                NftsInWallet[contract] = new List<int>();
            foreach (TokenData token in contractData.tokens)
            {
                NftsInWallet[contract].Add(int.Parse(token.token_id));
            }
        }

        return nftData;
    }


    public async UniTask<Dictionary<NftContract, int>> GetNftCounts(string walletAddress)
    {
        Dictionary<NftContract, int> returnDict = new Dictionary<NftContract, int>();
        await GetNftsInWallet(walletAddress);
        foreach (NftContract contract in Enum.GetValues(typeof(NftContract)))
        {
            if (contract == NftContract.None) continue;
            if (!NftsInWallet.ContainsKey(contract))
            {
                returnDict[contract] = 0;
                continue;
            }
            returnDict[contract] = NftsInWallet[contract].Count;
        }

        return returnDict;
    }
}