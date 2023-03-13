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

    public string ActiveWallet { get; private set; }
    public bool WalletVerified { get; private set; } = false;
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
        bool ownWallet = await ConfirmActiveWalletOwnership();
        WalletStatusModified.Invoke();
        if (!ownWallet)
        {
            return;
        }

        RawWalletData walletData = await GetNftsInWallet(ActiveWallet);
        WalletStatusModified.Invoke();
        if (NftsInWallet.Keys.Count == 0)
        {
            // Could not get knights
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "ERROR: Could not gather wallet contents. Please try again later.", () => { });
            return;
        }

        if (NftsInWallet[NftContract.Knights].Count <= 0 && NftsInWallet[NftContract.Villager].Count <= 0 &&
            NftsInWallet[NftContract.BlessedVillager].Count <= 0)
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

    public async UniTask ConnectWallet()
    {
#if UNITY_EDITOR
        SetWallet(GameSettings.EDITOR_WALLET);
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
            Debug.LogWarning($"[NftManager] Can not sign message without a wallet.");
            return false;
        }
#if UNITY_EDITOR
        Debug.Log($"[NftManager] Skipping Ownership Verification.");
        return true;
#endif
        // TODO: Check backend if wallet was previously authorized and is still valid.

        var message =
            $"Hello, welcome to Knights of the Ether.\nPlease sign this message to verify your wallet.\nThis action will not cost you any transaction fee.\n\n\nSecret Code: {Guid.NewGuid()}";
        WalletSignature walletSignature = new WalletSignature(ActiveWallet, message);
        Debug.Log($"[NftManager] Signing Message:\n{message}");
        bool signSuccessful = await walletSignature.SignWallet();
        if (!signSuccessful)
        {
            Debug.LogWarning($"[NftManager] Could not get wallet signature. Wallet not verified.");
            return false;
        }

        if (!await FetchData.Instance.VerifyWallet(walletSignature))
        {
            Debug.Log($"[NftManager] Wallet was not verified by backend.");
            return false;
        }

        Debug.LogWarning($"[NftManager] Wallet signature verified!");
        return true;
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
        WalletVerified = false;
        ActiveWallet = newAddress;
        NftsInWallet.Clear();
    }

    public async UniTask<RawWalletData> GetNftsInWallet(string walletAddress)
    {
        Debug.Log($"[WalletManager] Fetching Wallet Contents...");
        RawWalletData nftData = await FetchData.Instance.GetNftsInWallet(walletAddress);
        Debug.Log($"[WalletManager] Wallet Contents Received.");
        foreach (ContractData contractData in nftData.tokens)
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


    public async UniTask<int> GetNftCountPerContract(NftContract contract, string walletAddress)
    {
        await GetNftsInWallet(walletAddress);
        return NftsInWallet[contract].Count;
    }
}