using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using KOTE.UI.Armory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class FetchData : DataManager, ISingleton<FetchData>
{
    private static FetchData instance;

    public Dictionary<FetchType, string> TestData = new();

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
            rawJson = TryGetTestData(FetchType.ServerVersion, rawJson);

            return ParseJsonWithPath<string>(rawJson, "data");
        }
    }

    public async UniTask<List<Card>> GetCardUpgradePair(string cardId)
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.GetCardUpgradePair, cardId);
        rawJson = TryGetTestData(FetchType.UpgradePair, rawJson);

        return ParseJsonWithPath<List<Card>>(rawJson, "data.data.deck");
    }

    public async UniTask<CardUpgrade> CampUpgradeCard(string cardId)
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.UpgradeCard, cardId);
        rawJson = TryGetTestData(FetchType.UpgradeCard, rawJson);

        return ParseJsonWithPath<CardUpgrade>(rawJson, "data.data");
    }

    public async UniTask<List<Card>> GetUpgradeableCards()
    {
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData,
                WS_DATA_REQUEST_TYPES.UpgradableCards.ToString());
        rawJson = TryGetTestData(FetchType.UpgradeableCards, rawJson);

        return ParseJsonWithPath<List<Card>>(rawJson, "data.data");
    }

    public async UniTask<MerchantData> GetMerchantData()
    {
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.MerchantData.ToString());
        rawJson = TryGetTestData(FetchType.MerchantData, rawJson);

        return ParseJsonWithPath<MerchantData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> GetEncounterData()
    {
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.EncounterData.ToString());
        rawJson = TryGetTestData(FetchType.EncounterData, rawJson);

        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    public async UniTask<EncounterData> SelectEncounterOption(int option)
    {
        string rawJson = await socketRequest.EmitAwaitResponse(SocketEvent.EncounterSelection, option);
        rawJson = TryGetTestData(FetchType.EncounterOption, rawJson);

        return ParseJsonWithPath<EncounterData>(rawJson, "data.data");
    }

    public async UniTask<bool> VerifyWallet(WalletSignature walletSignature)
    {
        WWWForm form = walletSignature.ToWWWForm();
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.VerifyWalletSignature);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await MakeJsonRequest(request);
            rawJson = TryGetTestData(FetchType.VerifyWallet, rawJson);

            return ParseJsonWithPath<bool>(rawJson, "data.isValid");
        }
    }

    public async UniTask<RawWalletData> GetNftsInWallet(string wallet)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.WalletData) + $"/{wallet}";
        Debug.Log(requestUrl);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await KeepRetryingRequest(request);
            rawJson = TryGetTestData(FetchType.WalletNfts, rawJson);

            Debug.Log(rawJson);
            return ParseJsonWithPath<RawWalletData>(rawJson, "data");
        }
    }

    public async UniTask<GearData> GetGearInventory()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.PlayerGear);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            Debug.Log($"Raw Gear Data: {rawJson}");
            rawJson = TryGetTestData(FetchType.GearInventory, rawJson);

            if (string.IsNullOrEmpty(rawJson))
                return new GearData { ownedGear = new() /*, equippedGear = new()*/ };
            return ParseJsonWithPath<GearData>(rawJson, "data");
        }
    }

    public async UniTask<bool> RequestNewExpedition(NftContract characterType, int selectedNft,
        List<GearItemData> equippedGear)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionRequest);

        ExpeditionStartData startData = new ExpeditionStartData
        {tokenType = characterType.ToString(),
            nftId = selectedNft,
            equippedGear = equippedGear
        };

        string data = JsonConvert.SerializeObject(startData);
        byte[] utf8String = Encoding.Default.GetBytes(data);


        using (UnityWebRequest request = new UnityWebRequest(requestUrl, "POST"))
        {
            request.AddAuthToken();
            var uploadHandler = new UploadHandlerRaw(utf8String);
            uploadHandler.contentType = $"application/json";
            request.uploadHandler = uploadHandler;
            request.downloadHandler = new DownloadHandlerBuffer();
            string rawJson = await MakeJsonRequest(request);

            if (string.IsNullOrEmpty(rawJson)) return false;
            rawJson = TryGetTestData(FetchType.NewExpedition, rawJson);

            return ParseJsonWithPath<bool>(rawJson, "data.expeditionCreated");
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
        if (rawJson != null)
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved",
                CommunicationDirection.Incoming, rawJson);
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
            {
                // soft fail when testing
                Debug.LogWarning(e);
                return default(T);
            }
#endif
            Debug.LogException(e);
            return default(T);
        }
    }

    private string TryGetTestData(FetchType type, string rawData)
    {
#if UNITY_EDITOR
        if (UnitTestDetector.IsInUnitTest) rawData = GetTestData(type, rawData);
#endif
        return rawData;
    }

    private string GetTestData(FetchType type, string rawData)
    {
        if (TestData.ContainsKey(type))
        {
            return TestData[type];
        }

        return rawData;
    }
}

public enum FetchType
{
    ServerVersion,
    UpgradePair,
    UpgradeCard,
    UpgradeableCards,
    MerchantData,
    EncounterData,
    EncounterOption,
    VerifyWallet,
    WalletNfts,
    NftMetadata,
    GearInventory,
    NewExpedition
}