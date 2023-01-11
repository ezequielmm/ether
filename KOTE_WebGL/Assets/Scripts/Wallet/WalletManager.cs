using UnityEngine;
using UnityEngine.Serialization;

public class WalletManager : MonoBehaviour
{
    public GameObject walletsContainer;

    [Space(20)]
    public GameObject informationContent;
    [FormerlySerializedAs("rowPrefab")] public GameObject walletDataPrefab;

    private WalletItem walletItem;

    private void Start()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerWalletsPanel);
        GameManager.Instance.EVENT_DISCONNECT_WALLET_PANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerDisconnectWalletConfirmPanel);
        GameManager.Instance.EVENT_WALLET_ADDRESS_RECEIVED.AddListener(OnWalletAddressReceived);
        GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.AddListener(OnWalletContentsReceived);
        
        //create an instance of the current connected wallet, in case we need to add the ability to display more
        GameObject currentRow = Instantiate(walletDataPrefab, informationContent.transform);
        walletItem = currentRow.GetComponent<WalletItem>();
        
        // hardcoded wallet data for testing, metamask doesn't exist in editor so we have to send a wallet id manually
        #if UNITY_EDITOR
        GameManager.Instance.EVENT_WALLET_ADDRESS_RECEIVED.Invoke("0xbd22537d05207e470A458773683041012ddcAB65");
#endif
    }

    public void OnWalletAddressReceived(string walletAddress)
    {
        walletItem.SetWalletAddress(walletAddress);
        
        GameManager.Instance.EVENT_REQUEST_WALLET_CONTENTS.Invoke(walletAddress);
    }

    private void OnWalletContentsReceived(WalletKnightIds knightIds)
    {
        walletItem.SetKnightCount(knightIds.data.Length);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(knightIds.data);
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