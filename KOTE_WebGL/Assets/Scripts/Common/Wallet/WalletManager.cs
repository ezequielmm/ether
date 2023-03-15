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

    public UnityEvent<string> NewWalletConfirmed { get; } = new();
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

        List<int> knights = await GetNftsInWalletPerContract(NftContract.KnightsOfTheEther);
        WalletStatusModified.Invoke();
        if (knights == null)
        {
            // Could not get knights
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "ERROR: Could not gather wallet contents. Please try again later.", () => { });
            return;
        }

        if (knights.Count <= 0)
        {
            // No Knights
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                "No Knights found in connected wallet.\n, Please try a different wallet.", () => { });
            return;
        }

        WalletVerified = true;
        NewWalletConfirmed.Invoke(ActiveWallet);
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
            Debug.LogWarning($"[WalletManager] Can not sign message without a wallet.");
            return false;
        }
#if UNITY_EDITOR
        Debug.Log($"[WalletManager] Skipping Ownership Verification.");
        return true;
#endif
        // TODO: Check backend if wallet was previously authorized and is still valid.

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

    public async UniTask<List<int>> GetNftsInWalletPerContract(NftContract contract)
    {
        return await GetNftsInWalletPerContract(contract, ActiveWallet);
    }

    public async UniTask<List<int>> GetNftsInWalletPerContract(NftContract contract, string walletAddress)
    {
        if (contract != NftContract.KnightsOfTheEther)
        {
            throw new NotImplementedException();
        }

        if (NftsInWallet.ContainsKey(contract))
        {
            return NftsInWallet[contract];
        }

        string contractAddress = GetNftContractAddress(contract);
        Debug.Log($"[WalletManager] Fetching Wallet Contents...");
        WalletData nftData = await FetchData.Instance.GetNftsInWalletPerContract(walletAddress, contractAddress);
        Debug.Log($"[WalletManager] Wallet Contents Received.");
        NftsInWallet[contract] = new List<int>();
        foreach (ContractData contractData in nftData.tokens)
        {
            foreach (TokenData token in contractData.tokens)
            {
                NftsInWallet[contract].Add(int.Parse(token.token_id));
            }
        }

        return NftsInWallet[contract];
    }

    public async UniTask<int> GetNftCountPerContract(NftContract contract)
    {
        return await GetNftCountPerContract(contract, ActiveWallet);
    }

    public async UniTask<int> GetNftCountPerContract(NftContract contract, string walletAddress)
    {
        List<int> nfts = await GetNftsInWalletPerContract(contract, walletAddress);
        if (nfts == null) return 0;
        return nfts.Count;
    }

    private static string GetNftContractAddress(NftContract contract)
    {
        return NftManager.GetNftContractAddress(contract);
    }
}

public class WalletData
{
    public List<ContractData> tokens;
}

public class ContractData
{
    public string contract_address;
    public int token_count;
    public List<TokenData> tokens;
}

[Serializable]
public class TokenData
{
    public string token_id;
    public string name;
}