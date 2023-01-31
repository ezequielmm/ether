using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Networking.UnityWebRequest;
using Random = UnityEngine.Random;

public class WalletManager : MonoBehaviour
{
    public GameObject walletsContainer;

    [Space(20)] public GameObject informationContent;
    [FormerlySerializedAs("rowPrefab")] public GameObject walletDataPrefab;

    private WalletItem walletItem;

    // store the current wallet id so we don't request the data more than once
    private string curWallet = "";
    
    // we need to store the list of ids to verify owned nfts
    private List<int> nftIds;

    // these are used to verify ownership of the expedition nft if the encounter check occurs before the wallet data is pulled
    private bool RunNftDataCheck;
    private int _selectedNft;

    // store data for checking if the wallet is whitelisted
    private float signRequest;
    private string message;

    private void Start()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerWalletsPanel);
        GameManager.Instance.EVENT_DISCONNECT_WALLET_PANEL_ACTIVATION_REQUEST.AddListener(
            ActivateInnerDisconnectWalletConfirmPanel);
        GameManager.Instance.EVENT_WALLET_ADDRESS_RECEIVED.AddListener(OnWalletAddressReceived);
        GameManager.Instance.EVENT_MESSAGE_SIGN.AddListener(OnSignReceived);
        GameManager.Instance.EVENT_WALLET_DISCONNECTED.AddListener(OnWalletDisconnected);
        GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.AddListener(OnWalletContentsReceived);
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionStatus);

        //create an instance of the current connected wallet, in case we need to add the ability to display more
        GameObject currentRow = Instantiate(walletDataPrefab, informationContent.transform);
        walletItem = currentRow.GetComponent<WalletItem>();
    }

    public void OnWalletAddressReceived(string walletAddress)
    {
        //if we've already requested the wallet, don't ask again.
        if (walletAddress == curWallet) return;
        
        walletItem.SetWalletAddress(walletAddress);
        curWallet = walletAddress;

        GameManager.Instance.EVENT_REQUEST_WALLET_CONTENTS.Invoke(walletAddress);
        CheckWhitelist();
    }


    private void CheckWhitelist()
    {
#if UNITY_EDITOR
        GameManager.Instance.EVENT_WHITELIST_CHECK_RECEIVED.Invoke(true);
        return;
#endif
        signRequest = DateTimeOffset.Now.ToUnixTimeSeconds();
        message = $"Hello, welcome to Knights of the Ether.\nPlease sign this message to verify your wallet.\nThis action will not cost you any transaction fee.\n\n\nSecret Code: {Guid.NewGuid()}";
        Debug.Log($"[WalletManager] Sign Message:\n{message}");
        MetaMaskAdapter.Instance.SignMessage(message);
    }

    private void OnSignReceived(string result)
    {
        Debug.Log($"[WalletManager] Sign Result: {result}");
        GameManager.Instance.EVENT_REQUEST_WHITELIST_CHECK.Invoke(signRequest, message, result, curWallet);
    }

    private void OnWalletDisconnected()
    {
        curWallet = "";
        walletItem.SetKnightCount(0);
        walletItem.SetWalletAddress("");
    }

    private void OnWalletContentsReceived(WalletKnightIds knightIds)
    {
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

    public void OnDisconnectConfirm()
    {
    }

    public void ActivateInnerDisconnectWalletConfirmPanel(bool activate, GameObject wallet)
    {
    }

    public void ActivateInnerDisconnectWalletConfirmPanel(bool activate)
    {
    }

    public void ActivateInnerWalletsPanel(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}