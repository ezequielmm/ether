using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FetchData : DataManager, ISingleton<FetchData>
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

    public void DestroyInstance()
    {
        instance = null;
    }

    public async UniTask<string> GetServerVersion()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ServerVersion);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await MakeJsonRequest(request);
            return ParseJsonWithPath<string>(rawJson, "data");
        }
    }

    public async UniTask<List<Card>> GetCardUpgradePair(string cardId) 
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetCardUpgradePair, cardId);
        return ParseJsonWithPath<List<Card>>(rawJson, "data.data.deck");
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

    private async UniTask<string> MakeJsonRequest(UnityWebRequest request)
    {
        string rawJson = (await webRequest.MakeRequest(request))?.text;
        if (rawJson != null)
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved", CommunicationDirection.Incoming, rawJson);
        return rawJson;
    }

    private async UniTask<Texture2D> MakeTextureRequest(UnityWebRequest request)
    {
        Texture2D texture = ((DownloadHandlerTexture)await webRequest.MakeRequest(request))?.texture;
        if (texture != null)
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved", CommunicationDirection.Incoming, "{\"message\":\"<Image File>\"}");
        return texture;
    }

    public static T ParseJsonWithPath<T>(string rawJson, string tokenPath) 
    {
        try
        {
            JObject json = JObject.Parse(rawJson);
            T data = json.SelectToken(tokenPath).ToObject<T>();
            return data;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return default(T);
        }
    } 
}
