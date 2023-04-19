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
        using (UnityWebRequest request = UnityWebRequest.Post(requestUrl, form))
        {
            string rawJson = await MakeJsonRequest(request);
            rawJson = TryGetTestData(FetchType.VerifyWallet, rawJson);

            return ParseJsonWithPath<bool>(rawJson, "data.isValid");
        }
    }

    public async UniTask<RawWalletData> GetNftsInWallet(string wallet)
    {
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogError("Wallet contents were requested without a valid wallet");
            return null;
        }

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

    public async UniTask<ExpeditionStartData> RequestNewExpedition(NftContract characterType, int selectedNft,
        List<GearItemData> equippedGear)
    {
        if (!UserDataManager.Instance.VerifyAccountExists()) return new ExpeditionStartData
        {
            expeditionCreated = false
        };
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionRequest);

        ExpeditionStartSendData startData = new ExpeditionStartSendData
        {
            tokenType = characterType.ToString(),
            nftId = selectedNft,
            equippedGear = equippedGear,
            walletId = WalletManager.Instance.ActiveWallet,
            contractId = NftManager.Instance.GetContractAddress(characterType)
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

            if (string.IsNullOrEmpty(rawJson)) return new ExpeditionStartData
            {
                expeditionCreated = false,
                reason = "No Data Received"
            };;
            rawJson = TryGetTestData(FetchType.NewExpedition, rawJson);

            return ParseJsonWithPath<ExpeditionStartData>(rawJson, "data");
        }
    }

    public async UniTask<Texture2D> GetTexture(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            return await MakeTextureRequest(request);
        }
    }

    public async UniTask<Texture2D> GetVillagerPortraitElement(string elementName)
    {
        string spriteName = elementName + ".png";
        string requestUrl = ClientEnvironmentManager.Instance.PortraitElementURL + spriteName;
        return await GetTexture(requestUrl);
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


    public async UniTask<string> GetTokenByLogin(string email, string hashedPassword)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.Login);
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", hashedPassword);
        using (UnityWebRequest request = UnityWebRequest.Post(requestUrl, form))
        {
            // Logging disabled to hide sensitive information (Password)
            ServerCommunicationLogger.Instance.DisableDataLogging();
            string rawJson = await MakeJsonRequest(request);
            ServerCommunicationLogger.Instance.EnableDataLogging();
            return ParseJsonWithPath<string>(rawJson, "data.token");
        }
    }

    public async UniTask<string> GetTokenByRegistration(string name, string email, string hashedPassword)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.Register);
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("email", email);
        form.AddField("email_confirmation", email);
        form.AddField("password", hashedPassword);
        form.AddField("password_confirmation", hashedPassword);
        using (UnityWebRequest request = UnityWebRequest.Post(requestUrl, form))
        {
            // Logging disabled to hide sensitive information (Password)
            ServerCommunicationLogger.Instance.DisableDataLogging();
            string rawJson = await MakeJsonRequest(request);
            ServerCommunicationLogger.Instance.EnableDataLogging();
            return ParseJsonWithPath<string>(rawJson, "data.token");
        }
    }

    public async UniTask Logout()
    {
        string loginUrl = webRequest.ConstructUrl(RestEndpoint.Logout);

        ServerCommunicationLogger.Instance.LogCommunication(
            $"Logout request. token: {AuthenticationManager.Instance.GetSessionToken()}",
            CommunicationDirection.Outgoing);

        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            request.AddAuthToken();

            string rawData = await MakeJsonRequest(request);

            ServerCommunicationLogger.Instance.LogCommunication(
                $"Logout request result: " +
                ((request.result == UnityWebRequest.Result.Success) ? request.downloadHandler.text : request.error),
                CommunicationDirection.Incoming);

            if (request.result != UnityWebRequest.Result.Success)
            {
                GameManager.Instance.EVENT_REQUEST_LOGOUT_COMPLETED.Invoke(request.error);
                return;
            }

            LogoutData logoutData = ParseJsonWithPath<LogoutData>(rawData);
            string message = logoutData.data.message;

            GameManager.Instance.EVENT_REQUEST_LOGOUT_COMPLETED.Invoke(message);
        }
    }

    public async UniTask<ProfileData> GetPlayerProfile()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.Profile);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                string pannelMessage = "Error Retrieving profile, please log in again.";
                string[] buttons = { "Return To Login screen", string.Empty };
                GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(pannelMessage, () =>
                {
                    AuthenticationManager.Instance.Logout();
                }, null, buttons);
            }
                return ParseJsonWithPath<ProfileData>(rawJson, "data");
        }
    }

    public async UniTask<ExpeditionStatus> GetExpeditionStatus()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionStatus);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.SetRequestHeader("Accept", "*/*");
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            return ParseJsonWithPath<ExpeditionStatus>(rawJson, "data");
        }
    }

    public async UniTask<ContestData> GetOngoingContest()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.CurrentContest);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.SetRequestHeader("Accept", "*/*");
            string rawJson = await MakeJsonRequest(request);
            return ParseJsonWithPath<ContestData>(rawJson, "data");
        }
    }

    public async UniTask<ScoreboardData> GetExpeditionScore()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionScore);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.SetRequestHeader("Accept", "*/*");
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            if (string.IsNullOrEmpty(rawJson)) return null;
            return ParseJsonWithPath<ScoreboardData>(rawJson, "data");
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
        return rawJson;
    }

    private async UniTask<Texture2D> MakeTextureRequest(UnityWebRequest request)
    {
        Texture2D texture = ((DownloadHandlerTexture)await webRequest.MakeRequest(request))?.texture;
        if (texture == null)
        {
            texture = new Texture2D(1, 1);
        }

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