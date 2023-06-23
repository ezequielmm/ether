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
    public void UpdateWalletInfo() 
    {
        
            AddWallet(WalletManager.Instance.ActiveWallet);
        
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

        Dictionary<NftContract, int> tokenCounts = await WalletManager.Instance.GetNftCounts(walletAddress);
        WalletItem wallet = CreateWalletItem(walletAddress, tokenCounts);
        wallets.Add(wallet);
    }

    private WalletItem CreateWalletItem(string walletAddress, Dictionary<NftContract, int> tokenCounts ) 
    {
        GameObject walletGameObject = Instantiate(walletDataPrefab, informationContent.transform);
        var walletItem = walletGameObject.GetComponent<WalletItem>();
        walletItem.Populate(walletAddress,tokenCounts );
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