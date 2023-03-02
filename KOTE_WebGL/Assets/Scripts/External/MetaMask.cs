using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class MetaMask : SingleTon<MetaMask>
{
    public bool HasMetamask => CheckForMetamaskInstance();

    List<UniPromise<JObject>> outstandingPromises = new();

    protected override void Awake()
    {
        gameObject.name = "MetaMask";
        base.Awake();
    }

    public async UniTask<string> RequestAccount()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debug.LogError($"[MetaMask | {gameObject.name}] Enviroment not in WebGL. Can not access MetaMask.");
        return null;
#endif
        if (!HasMetamask)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] MetaMask is not found in the browser. Be sure to install it!");
            return null;
        }

        var promise = CreatePromise();
        MetamaskSelectAccount(promise.Id.ToString(), "ReturnFromJavascript", gameObject.name);
        await promise.WaitForFufillment();

        return ProcessSimpleData<string>("accountSelected", promise.Data);
    }


    public async UniTask<string> SignMessageWithAccount(string message, string account)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Debug.LogError($"[MetaMask | {gameObject.name}] Enviroment not in WebGL. Can not access MetaMask.");
        return null;
#endif
        if (!HasMetamask)
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] MetaMask is not found in the browser. Be sure to install it!");
            return null;
        }

        var promise = CreatePromise();
        MetamaskPersonalSign(promise.Id.ToString(), account, message, "ReturnFromJavascript", gameObject.name);
        await promise.WaitForFufillment();

        return ProcessSimpleData<string>("signedMessage", promise.Data);
    }

    private T ProcessSimpleData<T>(string successVariabele, JObject data)
    {
        if (data.ContainsKey("successVariabele"))
        {
            T successValue = data.GetValue("successVariabele").Value<T>();
            return successValue;
        }
        if (data.ContainsKey("error"))
        {
            Debug.LogError(
                $"[MetaMask | {gameObject.name}] {data.GetValue("error").Value<string>()}");
        }
        return default(T);
    }

    private void AccountChanged(string account)
    {
        account = account.TrimStart('[').TrimEnd(']');
        if (string.IsNullOrEmpty(account))
        {
            Debug.Log("[MetaMask] Account Change Received, no account found. Received account data: " + account);
            GameManager.Instance.EVENT_WALLET_DISCONNECTED.Invoke();
            return;
        }
        account = account.Trim('"');
        Debug.Log("[MetaMask] Account Change Received, new account data: " + account);

        NftManager.Instance.SelectedAccountChanged(account);
    }



    public void ReturnFromJavascript(string resultJson)
    {
        Debug.Log(resultJson);
        var json = JObject.Parse(resultJson);
        string promiseIdString = json.GetValue("promiseId")?.Value<string>();
        Guid promiseId = Guid.Parse(promiseIdString);
        if (promiseId == null)
        {
            Debug.LogError($"[MetaMask] Javascript returned with empty promise ID.");
            return;
        }
        FulfillPromise(promiseId, json);
    }

    private void FulfillPromise(Guid promiseId, JObject data)
    {
        UniPromise<JObject> promise = outstandingPromises.Find(other => other.Id.Equals(promiseId));
        promise.FulfillRequest(data);
        outstandingPromises.Remove(promise);
    }

    private UniPromise<JObject> CreatePromise() 
    {
        var promise = new UniPromise<JObject>();
        outstandingPromises.Add(promise);
        return promise;
    }

    private bool CheckForMetamaskInstance()
    {
        bool metaMask = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        metaMask = IsMetamaskInstalled();
#endif
        return metaMask;
    }

    #region JavascriptServiceLayer

    [DllImport("__Internal")]
    private static extern bool HasUnityInstance();

    [DllImport("__Internal")]
    private static extern bool IsMetamaskInstalled();

    [DllImport("__Internal")]
    private static extern void MetamaskSelectAccount(string promiseId, string returnMethod, string gameobject = "MetaMask");

    [DllImport("__Internal")]
    private static extern void MetamaskPersonalSign(string promiseId, string account, string message, string returnMethod, string gameobject = "MetaMask");

    #endregion
}