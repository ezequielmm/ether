using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using KOTE.UI.Armory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData,
                WS_DATA_REQUEST_TYPES.UpgradableCards.ToString());
        return ParseJsonWithPath<List<Card>>(rawJson, "data.data");
    }

    public async UniTask<MerchantData> GetMerchantData()
    {
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.MerchantData.ToString());
        return ParseJsonWithPath<MerchantData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> GetEncounterData()
    {
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.EncounterData.ToString());
        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> SelectEncounterOption(int option)
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.EncounterSelection, option);
        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    public async UniTask<bool> VerifyWallet(WalletSignature walletSignature)
    {
        WWWForm form = walletSignature.ToWWWForm();
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.VerifyWalletSignature);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await MakeJsonRequest(request);
            return ParseJsonWithPath<bool>(rawJson, "data.isValid");
        }
    }

    public async UniTask<List<int>> GetNftsInWalletPerContract(string wallet, string contract)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.WalletData) + $"/{wallet}?contractId={contract}";
        Debug.Log(requestUrl);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await KeepRetryingRequest(request);
            return ParseJsonWithPath<List<int>>(rawJson, "data");
        }
    }

    public async UniTask<List<Nft>> GetNftMetaData(List<int> tokenId, NftContract contract)
    {
        var requestBatch = tokenId.Partition(OpenSeasRequstBuilder.MaxContentRequest);
        List<Nft> nftMetaDataList = new List<Nft>();
        foreach (var tokenList in requestBatch)
        {
            using (UnityWebRequest request = OpenSeasRequstBuilder.ConstructNftRequest(contract, tokenList.ToArray()))
            {
                string rawJson = await MakeJsonRequest(request);
                nftMetaDataList.AddRange(ParseJsonWithPath<List<Nft>>(rawJson, "assets"));
            }

            await UniTask.Yield();
        }

        foreach (var token in nftMetaDataList)
        {
            token.Contract = contract;
        }

        return nftMetaDataList;
    }

    public async UniTask<GearData> GetGearInventory()
    {
        string requestUrl =
            "https://api.dev.kote.robotseamonster.com" + RestEndpoint.PlayerGear;
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.SetRequestHeader("Authorization",
                $"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIxIiwianRpIjoiMjM0Y2NkNmFlMzc4YjgxNDE1MmU1NzAyNjFmY2RlMDEyNTViYmE1ODZkNGEzMjU1NWUyMTQ5Njg2YTkyNjlkZTI0OTUyNjI5ZTE1NmVkZmIiLCJpYXQiOjE2NzgxNTE4OTYuNDU1MjY5LCJuYmYiOjE2NzgxNTE4OTYuNDU1Mjc2LCJleHAiOjE3MDk3NzQyOTYuNDMyNTE1LCJzdWIiOiIyNDMiLCJzY29wZXMiOltdfQ.ESjM9kAh7h2PHv3w0HbzFHTCrBNIvKng9FnBQhNlILxITxX8C5xnoxrmyj5t2xZbBJJ50kA4NqajFesedeyWJPXr7L1QW_3YdHGYr2-F7c9vTUbtxYG28d1ZdsMUYFhPU9Yt9W5MtH9XpN9TIzDLDsWmakIq6zSwawtLnbbWnVTuj5cOVtW9gAyL05dBgb5lxf6eD5GVYF_QJ2EOMTLPoMlnBrGmG2FobtvBcJxhlgyV9vfo8WtwTlFKTmp21FmV9NVvAppgz0DYjEeYcGwUDmmZ8kjSLmOsiDlRsCPmYKtEFZiOc-i5EhQmzqBcfyeXOy6pmXNfQIFfAHupyrjh2HJZKya_mDoKc0KJanoXBBo3J6YEwxoUohv8sIGMjFLE-TAq7QvB0pz00LVoM4iPhxx_MqH0-wqUns8riw-mBdTFw7Kp3RDphRivP_DF4FWcuRObTBBuRBmdKs6gzmQOPhXP7utadLQwr_zHMS_X1i33WigOdOFmqa_63SjaLxPBJlEzWCd7cP-M9o2gojj3twNrM-hG1A8OzUljWKkEE38Ey5iOUZ86q1jNSXwBv4yFwQ8BkUoyhIQwIEGA_Z9NCVhR4Y0cL_gDTtZzUwYv7Lr34-mqIWigEGneQHysiDb9m2vpCF7QKwwrOYj1EjRxBrjBC-Gm72zNQmNlQCM68Vk");
            string rawJson = await MakeJsonRequest(request);
            Debug.Log($"Raw Gear Data: {rawJson}");
            if (string.IsNullOrEmpty(rawJson)) return new GearData { data = new List<GearItemData>() };
            return ParseJsonWithPath<GearData>(rawJson);
        }
    }

    public async UniTask<Texture2D> GetTexture(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            return await MakeTextureRequest(request);
        }
    }

    public async UniTask<Texture2D> GetNftSkinElement(TraitSprite spriteData)
    {
        string spriteName = spriteData.ImageName + ".png";
        string requestUrl = ClientEnvironmentManager.Instance.SkinURL + spriteName;
        return await GetTexture(requestUrl);
    }

    public async UniTask<Texture2D> GetArmoryGearImage(string gearName)
    {
        string spriteName = gearName + ".png";
        string requestUrl = ClientEnvironmentManager.Instance.GearIconURL + spriteName;
        return await GetTexture(requestUrl);
    }

    public async UniTask<bool> RequestNewExpedition(string characterType, int selectedNft,
        GearData equippedGear)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionRequest);

        WWWForm form = new WWWForm();
        form.AddField("class", characterType);
        form.AddField("nftId", selectedNft);
        form.AddField("gear", JsonConvert.SerializeObject(equippedGear));

        using (UnityWebRequest request = UnityWebRequest.Post(requestUrl, form))
        {
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            if (string.IsNullOrEmpty(rawJson)) return false;
            return ParseJsonWithPath<bool>(rawJson, "data.expeditionCreated");
        }
    }

    private async UniTask<string> KeepRetryingRequest(UnityWebRequest request, int tryLimit = 10,
        float retryDelaySeconds = 3)
    {
        bool successful = false;
        int trys = 0;
        int retryDelayInMiliseconds = Mathf.RoundToInt(retryDelaySeconds * 1000);

        string rawJson = null;
        while (!successful && trys < tryLimit)
        {
            rawJson = await MakeJsonRequest(request);
            successful = !string.IsNullOrEmpty(rawJson);
            trys++;
            await UniTask.Delay(retryDelayInMiliseconds);
        }

        return rawJson;
    }

    private async UniTask<string> MakeJsonRequest(UnityWebRequest request) 
    {
        string rawJson = (await webRequest.MakeRequest(request))?.text;
        if(rawJson != null)
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved", CommunicationDirection.Incoming, rawJson);
        return rawJson;
    }

    private async UniTask<Texture2D> MakeTextureRequest(UnityWebRequest request)
    {
        Texture2D texture = ((DownloadHandlerTexture)await webRequest.MakeRequest(request))?.texture;
        if (texture != null)
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved",
                CommunicationDirection.Incoming, "{\"message\":\"<Image File>\"}");
        return texture;
    }

    public static T ParseJsonWithPath<T>(string rawJson, string tokenPath = null) 
    {
        try
        {
            JObject json = JObject.Parse(rawJson);
            JToken token = json;
            if (!string.IsNullOrEmpty(tokenPath)) 
            {
                token = json.SelectToken(tokenPath);
            }
            T data = token.ToObject<T>();
            return data;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            if (UnitTestDetector.IsInUnitTest) 
            { // soft fail when testing
                Debug.LogWarning(e);
                return default(T);
            }
#endif
            Debug.LogException(e);
            return default(T);
        }
    }
}