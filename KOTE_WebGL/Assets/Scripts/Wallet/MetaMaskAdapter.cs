using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class MetaMaskAdapter : SingleTon<MetaMaskAdapter>
{
    private MetaMask metaMask;

    private void Start()
    {
        metaMask = MetaMask.Instance;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(metaMask);
    }

    public bool HasMetamask() 
    {
        return metaMask.HasMetamask;
    }

    public async void RequestWallet() 
    {
        string walletAddress = await metaMask.RequestAccount();

        if(walletAddress == null) 
        {
            Debug.Log($"[MetaMaskAdapter] Could not fetch wallet.");
            return;
        }
        Debug.Log($"[MetaMaskAdapter] Got Wallet. [{walletAddress}]");
        GameManager.Instance.EVENT_WALLET_ADDRESS_RECEIVED.Invoke(walletAddress);
    }

    public void SignMessage(string account, string message) 
    {
        UnityEvent requestFail = new UnityEvent();
        requestFail.AddListener(SignFail);

        UnityEvent<string> requestSuccess = new UnityEvent<string>();
        requestSuccess.AddListener(SignSuccess);

        metaMask.SignMessage(message);
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
