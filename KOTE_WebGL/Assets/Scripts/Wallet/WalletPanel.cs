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
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async UniTask<List<string>> GetVerifiedWallets()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
        int knightCount = await WalletManager.Instance.GetNftCountPerContract(NftContract.knight, walletAddress);
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

    public void ToggleInnerWalletContainer(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}