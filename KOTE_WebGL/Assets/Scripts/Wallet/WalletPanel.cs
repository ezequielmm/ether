using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WalletPanel : MonoBehaviour
{
    [SerializeField]
    GameObject walletsContainer;

    [Space(20)] [SerializeField] 
    GameObject informationContent;

    [FormerlySerializedAs("rowPrefab")][SerializeField] 
    GameObject walletDataPrefab;

    List<WalletItem> wallets = new List<WalletItem>();

    private void Start()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ToggleInnerWalletContainer);
        GameManager.Instance.EVENT_MESSAGE_SIGN.AddListener(OnSignReceived);
        GameManager.Instance.EVENT_WALLET_DISCONNECTED.AddListener(OnWalletDisconnected);
        GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.AddListener(OnWalletContentsReceived);
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionStatus);

        //create an instance of the current connected wallet, in case we need to add the ability to display more
        GameObject currentRow = Instantiate(walletDataPrefab, informationContent.transform);
        walletItem = currentRow.GetComponent<WalletItem>();
        
    }

    private async UniTask<List<string>> GetVerifiedWallets() 
    {
        // TODO: Get previously connected wallets. For now, we don't persist connections
        // so this section stays blank. This will require backend support.
        return new List<string>();
    }

    public void ConnectNewWallet(string walletAddress)
    {
        AddWallet(walletAddress);
    }

    public void RemoveWallet(string walletAddress) 
    {
        if(!HasWallet(walletAddress)) { }
        WalletItem wallet = GetWallet(walletAddress);
        wallets.Remove(wallet);
        Destroy(wallet.gameObject);
    }

    private void AddWallet(string walletAddress) 
    {
        if(HasWallet(walletAddress)) 
        {
            return;
        }
        // Get Wallet Nft Data

        // Create Prefab

        // Add prefab to list
    }

    private bool HasWallet(string walletAddress) 
    {
        return wallets.Find(other => other.WalletAddress == walletAddress) != null;
    }

    private WalletItem GetWallet(string walletAddress) 
    {
        return wallets.Find(other => other.WalletAddress == walletAddress);
    }


    private void CheckWhitelist()
    {
#if UNITY_EDITOR
        GameManager.Instance.EVENT_WHITELIST_CHECK_RECEIVED.Invoke(true);
        return;
#endif
        GameManager.Instance.EVENT_WHITELIST_CHECK_RECEIVED.Invoke(false);
        signRequest = DateTimeOffset.Now.ToUnixTimeSeconds();
        message = $"Hello, welcome to Knights of the Ether.\nPlease sign this message to verify your wallet.\nThis action will not cost you any transaction fee.\n\n\nSecret Code: {Guid.NewGuid()}";
        Debug.Log($"[WalletManager] Sign Message:\n{message}");
        MetaMaskAdapter.Instance.SignMessage(message);
    }

    private void OnSignReceived(string result)
    {
        Debug.Log($"[WalletManager] Sign Result: {result}");
        GameManager.Instance.EVENT_REQUEST_WHITELIST_CHECK.Invoke(signRequest, message, result, activeWallet);
    }

    private void OnWalletDisconnected()
    {
        activeWallet = "";
        knightCount = 0;
        walletItem.SetKnightCount(0);
        walletItem.SetWalletAddress("");
    }

    private void OnWalletContentsReceived(WalletKnightIds knightIds)
    {
        if (knightIds.data == null || knightIds.data.Length == 0)
        {
            GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("No Knights found in connected wallet\n, please try a different wallet.", () => {});
            GameManager.Instance.EVENT_WALLET_TOKENS_OWNED.Invoke(false);
            return;
        }

        GameManager.Instance.EVENT_WALLET_TOKENS_OWNED.Invoke(true);
        knightCount = knightIds.data.Length;
        walletItem.SetKnightCount(knightIds.data.Length);
        nftIds = new List<int>();
        nftIds.AddRange(knightIds.data);
        if (RunNftDataCheck)
        {
            bool hasNft = VerifyNftInWallet(_selectedNft);
            RequestMetadata(hasNft);
        }
    }

    private void OnExpeditionStatus(bool hasExpedition, int nftId)
    {
        if (nftIds == null)
        {
            _selectedNft = hasExpedition ? nftId : -1;
            RunNftDataCheck = true;
            return;
        }

        // if there's no expedition, default to requesting all the data
        if (!hasExpedition)
        {
            RequestMetadata(false);
            return;
        }

        _selectedNft = nftId;
        // else only pull metadata for the selected nft
        bool hasNft = VerifyNftInWallet(_selectedNft);
        RequestMetadata(hasNft);
    }

    private bool VerifyNftInWallet(int nftId)
    {
        // if the nft is in the wallet, we only need the metadata for that single nft
        if (nftIds.Contains(nftId))
        {
            GameManager.Instance.EVENT_OWNS_CURRENT_EXPEDITION_NFT.Invoke(true);
            return true;
        }

        GameManager.Instance.EVENT_OWNS_CURRENT_EXPEDITION_NFT.Invoke(false);
        return false;
    }

    private void RequestMetadata(bool requestSingle)
    {
        if (requestSingle)
        {
            GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(new int[] { _selectedNft} );
            return;
        }

        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(nftIds.ToArray());
    }

    public void ToggleInnerWalletContainer(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}