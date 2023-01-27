using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WalletItem : MonoBehaviour
{
    public TMP_Text walletAddressText;
    public TMP_Text knightCountText;

    public void SetWalletAddress(string address)
    {
        walletAddressText.text = address;
    }

    public void SetKnightCount(int count)
    {
        knightCountText.text = count + " Knights";
    }

    public void ActivateConfirmation()
    {
        GameManager.Instance.EVENT_DISCONNECT_WALLET_PANEL_ACTIVATION_REQUEST.Invoke(true, gameObject);
    }
}