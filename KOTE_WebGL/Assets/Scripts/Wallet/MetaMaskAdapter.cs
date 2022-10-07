using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MetaMaskAdapter : MonoBehaviour
{
    private MetaMask mm;

    private void Start()
    {
        mm = MetaMask.Instance;
    }

    public bool HasMetamask() 
    {
        return mm.HasMetamask;
    }

    public void RequestWallet() 
    {
        UnityEvent requestFail = new UnityEvent();
        requestFail.AddListener(GetWalletFail);

        UnityEvent<string> requestSuccess = new UnityEvent<string>();
        requestSuccess.AddListener(GetWalletSuccess);

        mm.GetAccount(requestSuccess, requestFail);
    }

    public void SignMessage(string message) 
    {
        UnityEvent requestFail = new UnityEvent();
        requestFail.AddListener(GetWalletFail);

        UnityEvent<string> requestSuccess = new UnityEvent<string>();
        requestSuccess.AddListener(GetWalletSuccess);

        mm.GetAccount(requestSuccess, requestFail);
    }

    private void GetWalletFail() 
    {
        Debug.LogError($"[MetaMaskAdapter] Could not get Wallet Address.");
    }
    private void GetWalletSuccess(string wallet)
    {
        Debug.Log($"[MetaMaskAdapter] Got Wallet. [{wallet}]");
        GameManager.Instance.EVENT_NEW_WALLET.Invoke(wallet);
    }

    private void SignFail()
    {
        Debug.LogError($"[MetaMaskAdapter] Could not sign message.");
    }
    private void SignSuccess(string result) 
    {
        Debug.Log($"[MetaMaskAdapter] Message Signed. [{result}]");
        GameManager.Instance.EVENT_MESSAGE_SIGN.Invoke(result);
    }
}
