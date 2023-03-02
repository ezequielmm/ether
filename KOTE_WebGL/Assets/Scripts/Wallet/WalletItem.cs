using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WalletItem : MonoBehaviour
{
    public string WalletAddress { get; private set; }
    public int KnightCount { get; private set; }
    public bool IsActiveWallet => WalletManager.Instance.ActiveWallet == WalletAddress;

    [SerializeField]
    private TMP_Text walletAddressText;
    [SerializeField]
    private TMP_Text knightCountText;

    public void SetWalletAddress(string wallet)
    {
        walletAddressText.text = wallet;
        WalletAddress = wallet;
    }
    public void SetKnightCount(int count)
    {
        knightCountText.text = count + " Knights";
        KnightCount = count;
    }
    public void Populate(string wallet, int knights) 
    {
        SetWalletAddress(wallet);
        SetKnightCount(knights);
    }

    public void RemoveWallet()
    {
        WalletManager.Instance.ForgetWallet(WalletAddress);
    }
}