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
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.AddListener(OnExpeditionStatus);

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
        if(!HasWallet(walletAddress)) 
        {
            return;
        }
        WalletItem wallet = GetWallet(walletAddress);
        wallets.Remove(wallet);
        Destroy(wallet.gameObject);
    }

    private async void AddWallet(string walletAddress) 
    {
        if(HasWallet(walletAddress)) 
        {
            return;
        }
        int knightCount = await WalletManager.Instance.GetNftCountPerContract(NftContract.KnightsOfTheEther, walletAddress);
        WalletItem wallet = CreateWalletItem(walletAddress, knightCount);
        wallets.Add(wallet);
    }

    private WalletItem CreateWalletItem(string walletAddress, int knightCount) 
    {
        GameObject walletGameObject = Instantiate(walletDataPrefab, informationContent.transform);
        var walletItem = walletGameObject.GetComponent<WalletItem>();
        walletItem.Populate(walletAddress, knightCount);
        return walletItem;
    }

    private bool HasWallet(string walletAddress) 
    {
        return wallets.Find(other => other.WalletAddress == walletAddress) != null;
    }

    private WalletItem GetWallet(string walletAddress) 
    {
        return wallets.Find(other => other.WalletAddress == walletAddress);
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