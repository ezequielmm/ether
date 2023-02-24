using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FetchData : ISingleton<FetchData>, IDisposable
{
    private static FetchData instance;
    public static FetchData Instance 
    { 
        get 
        {
            if (instance == null) 
            {
                instance = new FetchData();
            }
            return instance;
        } 
    }
    WebRequesterManager webRequest;
    WebSocketManager socketRequest;

    private FetchData() 
    {
        GameManager.Instance.EVENT_SCENE_LOADED.AddListener(OnSceneLoaded);
        OnSceneLoaded(GameManager.Instance.CurrentScene);
    }

    private void OnSceneLoaded(inGameScenes scene) 
    {
        switch(scene) 
        {
            case inGameScenes.MainMenu:
                webRequest = GameObject.FindObjectOfType<WebRequesterManager>();
                break;
            case inGameScenes.Expedition:
                socketRequest = WebSocketManager.Instance;
                break;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }
    public void Dispose()
    {
        
    }

    public async UniTask<string> GetServerVersion()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ServerVersion);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await webRequest.MakeRequest(request);
            return ParseJsonWithPath<string>(rawJson, "data");
        }
    }

    public async UniTask<Deck> GetCardUpgradePair(string cardId) 
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetCardUpgradePair, cardId);
        return ParseJsonWithPath<Deck>(rawJson, "data.data");
    }

    public async UniTask<CardUpgrade> CampUpgradeCard(string cardId)
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.UpgradeCard, cardId);
        return ParseJsonWithPath<CardUpgrade>(rawJson, "data.data");
    }

    public async UniTask<List<Card>> GetUpgradeableCards()
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.UpgradableCards.ToString());
        return ParseJsonWithPath<List<Card>>(rawJson, "data.data");
    }

    public async UniTask<MerchantData> GetMerchantData()
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.MerchantData.ToString());
        return ParseJsonWithPath<MerchantData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> GetEncounterData()
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.EncounterData.ToString());
        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> SelectEncounterOption(int option) 
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.EncounterSelection, option);
        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    private T ParseJsonWithPath<T>(string rawJson, string tokenPath) 
    {
        JObject json = JObject.Parse(rawJson);
        T data = json.SelectToken(tokenPath).ToObject<T>();
        return data;
    } 
}
