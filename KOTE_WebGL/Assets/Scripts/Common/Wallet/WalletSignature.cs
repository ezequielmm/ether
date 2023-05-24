using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletSignature
{
    public long UnixTimeOfRequest { get; private set; }
    public string WalletAddress { get; private set; }
    public string Signature { get; private set; }
    public string OriginalMessage { get; private set; }

    public WalletSignature(string walletAddress, string originalMessage)
    {
        WalletAddress = walletAddress;
        OriginalMessage = originalMessage;
    }

    public async UniTask<bool> SignWallet()
    {
        return true;
        /*
        Lea: Hardcoded signature sign. 
        UnixTimeOfRequest = DateTimeOffset.Now.ToUnixTimeSeconds();
        Signature = await MetaMask.Instance.SignMessageWithAccount(OriginalMessage, WalletAddress);
        return !string.IsNullOrEmpty(Signature);
        */

    }

    public WWWForm ToWWWForm()
    {
        WWWForm form = new WWWForm();
        form.AddField("sig", Signature); // The 0x signature string
        form.AddField("wallet", WalletAddress); // The 0x wallet string
        form.AddField("created", (int)UnixTimeOfRequest); // Unix Timestamp
        form.AddField("message", OriginalMessage); // String of what was signed
        return form;
    }
}
