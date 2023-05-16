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

    public UnityEvent<RawWalletData> NewWalletConfirmed { get; } = new();
    public UnityEvent<string> DisconnectingWallet { get; } = new();
    public UnityEvent WalletStatusModified { get; } = new();

    public string ActiveWallet { get; set; }
    public bool WalletVerified { get; set; } = false;
    public Dictionary<NftContract, List<int>> NftsInWallet = new();

    private MetaMask metaMask;

    private WalletManager()
    {
        metaMask = MetaMask.Instance;
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
            return;
        }

        RawWalletData walletData = await GetNftsInWallet(ActiveWallet);
        
        WalletStatusModified.Invoke();
        if (walletData == null)
        {
            Debug.LogWarning("No Wallet Contents retrieved");
        }
        if (NftsInWallet.Keys.Count == 0)
        {
            // Could not get knights
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "ERROR: Could not gather wallet contents. Please try again later.", () => { });
            return;
        }

        
        
        if (!WalletHasNfts())
        {
            // No Knights
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "No KOTE tokens found in connected wallet.\n, Please try a different wallet.", () => { });
            return;
        }

        WalletVerified = true;
        NewWalletConfirmed.Invoke(walletData);
        WalletStatusModified.Invoke();
    }

    private bool WalletHasNfts()
    {
        bool hasNfts = false;
        foreach (NftContract contract in NftsInWallet.Keys)
        {
            if (NftsInWallet[contract] != null && NftsInWallet[contract].Count > 0)
            {
                hasNfts = true;
            } 
        }

        return hasNfts;
    }

    public async UniTask ConnectWallet()
    {
#if UNITY_EDITOR
        SetWallet(AuthenticationManager.Instance.Wallet);
        return;
#endif
        string activeAccount = await metaMask.RequestAccount();
        if (activeAccount == null)
        {
            return;
        }

        SetWallet(activeAccount);
    }


    public async UniTask<bool> ConfirmActiveWalletOwnership()
    {
        if (string.IsNullOrEmpty(ActiveWallet))
        {
            Debug.LogWarning($"[WalletManager] Can not sign message without a wallet.");
            return false;
        }
#if UNITY_EDITOR
        Debug.Log($"[WalletManager] Skipping Ownership Verification.");
        return true;
#endif
        // TODO: Check backend if wallet was previously authorized and is still valid.
        if (WalletVerified) return true;
        var message =
            $"Hello, welcome to Knights of the Ether.\nPlease sign this message to verify your wallet.\nThis action will not cost you any transaction fee.\n\n\nSecret Code: {Guid.NewGuid()}";
        WalletSignature walletSignature = new WalletSignature(ActiveWallet, message);
        Debug.Log($"[WalletManager] Signing Message:\n{message}");
        bool signSuccessful = await walletSignature.SignWallet();
        if (!signSuccessful)
        {
            Debug.LogWarning($"[WalletManager] Could not get wallet signature. Wallet not verified.");
            return false;
        }

        if (!await FetchData.Instance.VerifyWallet(walletSignature))
        {
            Debug.Log($"[WalletManager] Wallet was not verified by backend.");
            return false;
        }

        Debug.LogWarning($"[WalletManager] Wallet signature verified!");
        return true;
    }

    public bool ConfirmNftOwnership(int nftId, NftContract tokenType)
    {
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
        return false;
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
        if (newAddress != ActiveWallet)
        {
            WalletVerified = false;
            ActiveWallet = newAddress;
        }

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
            NftManager.Instance.SetContractAddress(contract, contractData.contract_address);

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