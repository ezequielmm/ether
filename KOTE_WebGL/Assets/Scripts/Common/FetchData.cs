using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using IMBX;
using KOTE.UI.Armory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDev;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class FetchData : DataManager, ISingleton<FetchData>
{
    private static FetchData instance;

    public Dictionary<FetchType, string> TestData = new();

    private uint textureIndex;
    
    private Dictionary<string, Texture2D> cachedTextures = new();

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

    public void CleanMemory(Texture2D[] ignoreTextures)
    {
        var copy = cachedTextures.Values.ToList();
        foreach (var texture2D in copy)
        {
            if (!ignoreTextures.Contains(texture2D))
                MonoBehaviour.Destroy(texture2D);
        }
        cachedTextures.Clear();
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
        Debug.Log("Get encounter data here");
        string rawJson =
            await socketRequest.EmitAwaitResponse(SocketEvent.GetData, WS_DATA_REQUEST_TYPES.EncounterData.ToString());
        rawJson = TryGetTestData(FetchType.EncounterData, rawJson);
        Debug.Log("Found encounter data");
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

        int amount = 10;
        if (URLParameters.GetSearchParameters().TryGetValue("amount", out var nftsAmountString))
            amount = int.Parse(nftsAmountString) < 5 ? 5 : int.Parse(nftsAmountString);
        
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.WalletData) + $"/{wallet}?amount={amount}";
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

    public async UniTask<ExpeditionStartData> RequestNewExpedition(string characterType, int selectedNft,
        List<VictoryItems> equippedGear)
    {
        /*
         
         
        */
        if (!UserDataManager.Instance.VerifyAccountExists()) return new ExpeditionStartData
        {
            expeditionCreated = false
        };
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionRequest);

        ExpeditionStartSendData startData = new ExpeditionStartSendData
        {
            tokenType = characterType,
            nftId = selectedNft,
            equippedGear = equippedGear,
            walletId = WalletManager.Instance.ActiveWallet,
            contractId = characterType
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
    
    public void GetTexture(string url, Action<Texture2D> onResolve)
    {
        string imageName = GetMD5Hash(url);
        if (url.EndsWith(".jpg"))
            imageName += ".jpg";
        else
            imageName += ".png";

        if (cachedTextures.ContainsKey(imageName))
        {
            onResolve?.Invoke(cachedTextures[imageName]);
            return;
        }

        var loader = ImageLoader.Create(0);
        
        var myIndex = textureIndex++;

        bool textureLoaded = false;
        
        loader.Load(
            myIndex,
            url,
            imageName,
            "CachedImages",
            ImageLoader.CacheMode.NoCache,
            (loadedText, index) =>
                {
                    if (myIndex == index)
                    {
                        if (cachedTextures.ContainsKey(imageName)) {
                            onResolve?.Invoke(cachedTextures[imageName]);
                            return;
                        }
                        
                        textureLoaded = true;
                        if (loadedText)
                            loadedText.name = url;
                        cachedTextures.Add(imageName, loadedText);
                        onResolve?.Invoke(loadedText);
                    }
                }
            );
    }
    
    private string GetMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }

    public void GetNftSkinElement(TraitSprite spriteData, Action<Texture2D> callback)
    {
        string spriteName = spriteData.ImageName + ".png";
        string requestUrl = ClientEnvironmentManager.Instance.SkinURL.AddPath(spriteName);
        GetTexture(requestUrl, callback);
    }

    public void GetArmoryGearImage(Trait gearType, string gearName, Action<Texture2D> callback)
    {
        gearName = $"{gearType}/{gearName}";
        string spriteName = gearName + ".jpg";
        string requestUrl = ClientEnvironmentManager.Instance.GearIconURL.AddPath(spriteName);
        GetTexture(requestUrl, callback);
    }
    
    public void GetKnightPortrait(int tokenID, Action<Texture2D> callback)
    {
        string requestUrl = ClientEnvironmentManager.Instance.KnightPortraitsURL.AddPath($"{tokenID}.jpg");
        GetTexture(requestUrl, callback);
    }

    public void GetLootboxGearImage(string image, Action<Texture2D> action)
    {
        if (!image.EndsWith(".png"))
            image += ".png";
        var url = ClientEnvironmentManager.Instance.LootboxAssetsURL.AddPath(image);
        GetTexture(url, action);
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
    void ClearScene()
    {
        // Obtener una lista de todos los GameObjects en la escena
        var gameObjectList = GameObject.FindObjectsOfType<GameObject>();

        // Destruir todos los GameObjects en la lista
        //
        //
        //
        foreach (var item in gameObjectList)
        {
            GameObject.Destroy(item);  
        }
    }
    public async UniTask Logout()
    {
        /*
        if (populatedWithStatus == GameStatuses.ScoreBoardAndNextAct)
        {
            GameManager.Instance.LoadSceneNewTest();
            return;
        }
        */

        //LoadingManager.Won = false;
        //Cleanup.CleanupGame();

        //string _url = "https://test2.knightsoftheether.com/";

        //Application.OpenURL(_url);

        Application.ExternalCall("RecargarPagina");



        //// Get a list of all the objects that are in dontDestroyOnLoad
        //List<GameObject> gameObjectList = new List<GameObject>();
        //foreach (GameObject gameObject in GameObject.FindObjectsOfType(typeof(GameObject)))
        //{
        //    if (gameObject.DontDestroyOnLoad)
        //    {
        //        gameObjectList.Add(gameObject);
        //    }
        //}

        //// Destroy all the objects
        //Object.DestroyAll(gameObjectList);

        //GameObject.Destroy(WebSocketManager.Instance);

        //var go = GameObject.Find("WebSocketManager");
        //GameObject.Destroy(go);

        //ClearScene();



        //SceneManager.LoadScene(0);

        /*
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
        */
    }

    public async UniTask<ProfileData> GetPlayerProfile()
    {
        Debug.Log("Start get profile");
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.Profile);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            Debug.Log("Raw json " + rawJson);
            Debug.Log("Request result to check " + request.error);
            Debug.Log("Request " + request.result);
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("no profile");
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
#if UNITY_EDITOR
        SaveTextAsJson(rawJson);
#endif
        return rawJson;
    }
    public void SaveTextAsJson(string text)
    {
        // Create the directories if they don't exist
        string directoryPath = Path.Combine(Application.persistentDataPath, "responses", "jsons");
        Directory.CreateDirectory(directoryPath);

        // Generate a unique file name using a timestamp
        string fileName = $"response_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";

        // Combine the directory path and file name
        string filePath = Path.Combine(directoryPath, fileName);

        // Format the text as JSON
      

        // Save the JSON file
        File.WriteAllText(filePath, text);

    }
    private async UniTask<Texture2D> MakeTextureRequest(UnityWebRequest request)
    {
        Texture2D texture = ((DownloadHandlerTexture)await webRequest.MakeRequest(request, false))?.texture;
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
    
    public async UniTask<LeaderboardData> GetLeaderboardData()
    {
        await UniTask.Delay(3);
        return new LeaderboardData
        {
            data = new []
            {
                new LeaderboardDataItem("a",100),
                new LeaderboardDataItem("b",120),
                new LeaderboardDataItem("c",200),
                new LeaderboardDataItem("d",103),
                new LeaderboardDataItem("e",400)
            }
        };
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