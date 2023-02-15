using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BestHTTP.JSON;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Check HelperClasses.cs for the classes usaed to hold JSON data
/// </summary>
public class WebRequesterManager : MonoBehaviour
{
    private string baseUrl;
    private string skinUrl;
    private readonly string urlRandomName = "/auth/v1/generate/username";
    private readonly string urlRegister = "/auth/v1/register";
    private readonly string urlLogin = "/auth/v1/login";
    private readonly string urlLogout = "/auth/v1/logout";
    private readonly string urlProfile = "/gsrv/v1/profile";
    private readonly string urlWalletData = "/gsrv/v1/wallets";
    private readonly string urlKoteWhitelist = "/gsrv/v1/tokens/verify";
    private readonly string urlCharactersList = "/gsrv/v1/characters";
    private readonly string urlExpeditionStatus = "/gsrv/v1/expeditions/status";
    private readonly string urlExpeditionRequest = "/gsrv/v1/expeditions";
    private readonly string urlExpeditionCancel = "/gsrv/v1/expeditions/cancel";
    private readonly string urlExpeditionScore = "/gsrv/v1/expeditions/score";
    private readonly string urlServerVersion = "/gsrv/v1/showVersion";


    private readonly string urlOpenSea =
        "https://api.opensea.io/api/v1/assets?xxxx&asset_contract_address=0x32A322C7C77840c383961B8aB503c9f45440c81f&format=json";

    // we have to queue the requested nft images due to rate limiting
    private Queue<(string, string)> requestedNftImages = new Queue<(string, string)>();
    private bool nftQueueRunning;

    private void Awake()
    {
        HiddenConsoleManager.DisableOnBuild();

        // determine the correct server the client is running on
        string hostName = Application.absoluteURL;
        Debug.Log("hostName:" + hostName);

        baseUrl = "https://gateway.dev.kote.robotseamonster.com"; //make sure if anything fails we use DEV
        // baseUrl = "https://gateway.alpha.knightsoftheether.com";//make sure if anything fails we use DEV

        if (hostName.IndexOf("alpha") > -1 && hostName.IndexOf("knight") > -1)
        {
            baseUrl = "https://gateway.alpha.knightsoftheether.com";
            skinUrl = "https://s3.amazonaws.com/koteskins.knightsoftheether.com/";
            GameManager.ClientEnvironment = "Alpha";
        }
        
        if (hostName.IndexOf("alpha") > -1 && hostName.IndexOf("robot") > -1)
        {
            baseUrl = "https://gateway.alpha.kote.robotseamonster.com";
            skinUrl = "https://koteskins.robotseamonster.com/";
            GameManager.ClientEnvironment = "Alpha";
        }

        if (hostName.IndexOf("stage") > -1)
        {
            baseUrl = "https://gateway.stage.kote.robotseamonster.com";
            skinUrl = "https://koteskins.robotseamonster.com/";
            GameManager.ClientEnvironment = "Stage";
        }

        if (hostName.IndexOf("dev") > -1)
        {
            baseUrl = "https://gateway.dev.kote.robotseamonster.com";
            skinUrl = "https://koteskins.robotseamonster.com/";
            GameManager.ClientEnvironment = "Dev";
        }


        // default to the stage server if we're in the editor
#if UNITY_EDITOR
        baseUrl = "https://gateway.dev.kote.robotseamonster.com";
        skinUrl = "https://koteskins.robotseamonster.com/";
        GameManager.ClientEnvironment = "Unity";
#endif

        PlayerPrefs.SetString("api_url", baseUrl);

        Debug.Log("Base URL: " + baseUrl.ToString());
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();

        GameManager.Instance.EVENT_REQUEST_NAME.AddListener(OnRandomNameEvent);
        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener(OnRegisterEvent);
        GameManager.Instance.EVENT_REQUEST_LOGIN.AddListener(RequestLogin);
        GameManager.Instance.EVENT_REQUEST_PROFILE.AddListener(RequestProfile);
        GameManager.Instance.EVENT_REQUEST_WALLET_CONTENTS.AddListener(RequestWalletContents);
        GameManager.Instance.EVENT_REQUEST_LOGOUT.AddListener(RequestLogout);
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.AddListener(RequestExpeditionCancel);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.AddListener(RequestNftData);
        GameManager.Instance.EVENT_REQUEST_NFT_IMAGE.AddListener(RequestNftImage);
        GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.AddListener(RequestNftSkinElement);
        GameManager.Instance.EVENT_REQUEST_NFT_SET_SKIN.AddListener(SetKnightNft);
        GameManager.Instance.EVENT_REQUEST_EXPEDITON_SCORE.AddListener(RequestExpeditionScore);
        GameManager.Instance.EVENT_REQUEST_WHITELIST_CHECK.AddListener(RequestWhitelistStatus);
        GameManager.Instance.EVENT_REQUEST_SERVER_VERSION.AddListener(RequestServerVersion);
    }

    private void Start()
    {
        if (GameManager.Instance.webRequester == null)
        {
            GameManager.Instance.webRequester = this;
            DontDestroyOnLoad(this);
        }
        else if (GameManager.Instance.webRequester != this)
        {
            Destroy(this.gameObject);
        }
    }

    internal void RequestStartExpedition(string characterType, string selectedNft)
    {
        StartCoroutine(RequestNewExpedition(characterType, selectedNft));
    }

    public void RequestLogout(string token)
    {
        StartCoroutine(GetLogout(token));
    }

    public void RequestExpeditionStatus()
    {
        StartCoroutine(GetExpeditionStatus());
    }

    public void RequestExpeditionScore()
    {
        StartCoroutine(GetExpeditionScore());
    }

    public void RequestExpeditionCancel()
    {
        StartCoroutine(CancelOngoingExpedition());
    }

    public void RequestWhitelistStatus(float expires, string message, string signature, string wallet)
    {
        StartCoroutine(WhitelistStatus(expires, message, signature, wallet));
    }

    public void SetKnightNft(int tokenId)
    {
        StartCoroutine(GetSingleNft(tokenId));
    }

    public void RequestNftData(int[] tokenIds)
    {
        StartCoroutine(GetNftData(tokenIds));
    }

    public void RequestNftImage(NftMetaData[] requestedTokens)
    {
        foreach (NftMetaData metaData in requestedTokens)
        {
            requestedNftImages.Enqueue((metaData.token_id, metaData.image_url));
        }

        if (!nftQueueRunning)
        {
            StartCoroutine(GetNftImages());
        }
    }

    public void RequestNftSkinElement(TraitSprite spriteToPopulate)
    {
        StartCoroutine(GetNftSkinElement(spriteToPopulate));
    }

    public void OnRandomNameEvent(string previousName)
    {
        StartCoroutine(GetRandomName(previousName));
    }

    public void OnRegisterEvent(string name, string email, string password)
    {
        StartCoroutine(GetRegister(name, email, password));
    }

    public void RequestLogin(string email, string password)
    {
        StartCoroutine(GetLogin(email, password));
    }

    public void RequestProfile(string token)
    {
        StartCoroutine(GetProfile(token));
    }

    public void RequestWalletContents(string walletAddress)
    {
        StartCoroutine(GetWalletContents(walletAddress));
    }

    public void RequestCharacterList()
    {
        StartCoroutine(GetCharacterList());
    }

    public void RequestServerVersion(Action<string> resultCallback)
    {
        StartCoroutine(GetServerVersion(resultCallback));
    }

    public IEnumerator GetServerVersion(Action<string> resultCallback)
    {
        string serverVersionUrl = $"{baseUrl}{urlServerVersion}";

        using (UnityWebRequest request = UnityWebRequest.Get(serverVersionUrl)) 
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"{request.error}");
                yield break;
            }

            ServerVersionText serverVersionObj = JsonUtility.FromJson<ServerVersionText>(request.downloadHandler.text);
            string serverVersion = serverVersionObj.data;

            Debug.Log($"Server Version: [{serverVersion}]");

            resultCallback.Invoke(serverVersion);
        }
    }

    public IEnumerator GetRandomName(string lastName)
    {
        string randomNameUrl = $"{baseUrl}{urlRandomName}";

        UnityWebRequest randomNameInfoRequest;

        randomNameInfoRequest = UnityWebRequest.Get($"{randomNameUrl}?username={Uri.EscapeDataString(lastName)}");

        yield return randomNameInfoRequest.SendWebRequest();

        if (randomNameInfoRequest.result == UnityWebRequest.Result.ConnectionError ||
            randomNameInfoRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"{randomNameInfoRequest.error}");
            yield break;
        }

        //TODO: check for errors here

        RandomNameData randomNameData =
            JsonUtility.FromJson<RandomNameData>(randomNameInfoRequest.downloadHandler.text);
        string newName = randomNameData.data.username;

        GameManager.Instance.EVENT_REQUEST_NAME_SUCESSFUL.Invoke(string.IsNullOrEmpty(newName) ? "" : newName);
    }


    /// <summary>
    /// GetRegister
    /// </summary>
    /// <param name="nameText"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public IEnumerator GetRegister(string nameText, string email, string password)
    {
        string registerUrl = $"{baseUrl}{urlRegister}";
        WWWForm form = new WWWForm();
        form.AddField("name", nameText);
        form.AddField("email", email);
        form.AddField("email_confirmation", email);
        form.AddField("password", password);
        form.AddField("password_confirmation", password);

        using (UnityWebRequest request = UnityWebRequest.Post(registerUrl, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"{request.error}");
                yield break;
            }

            RegisterData registerData = JsonUtility.FromJson<RegisterData>(request.downloadHandler.text);
            string token = registerData.data.token;

            Debug.Log("Registration sucessful, token is " + token);

            //TO DO: check for errors even on a sucessful answer

            GameManager.Instance.EVENT_REQUEST_PROFILE
                .Invoke(token); //we request the profile to confirm the server got our account created properly. This will invoke later EVENT_LOGIN_COMPLETED
        }
    }

    /// <summary>
    /// GetLogin
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="rememberMe"></param>
    /// <returns></returns>
    IEnumerator GetLogin(string email, string password)
    {
        string loginUrl = $"{baseUrl}{urlLogin}";

        Debug.Log("Loing url:" + loginUrl);

        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"{request.error}");
                GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke(request.error);
                yield break;
            }

            LoginData loginData = JsonUtility.FromJson<LoginData>(request.downloadHandler.text);
            string token = loginData.data.token;

            //TODO: check for errors even on successful result

            GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token);
        }
    }

    IEnumerator GetProfile(string token)
    {
        // Debug.Log("Getting profile with token " + token);

        string profileUrl = $"{baseUrl}{urlProfile}";

        //UnityWebRequest profileInfoRequest = UnityWebRequest.Get($"{profileUrl}?Authorization={Uri.EscapeDataString(token)}");
        using (UnityWebRequest
               request = UnityWebRequest
                   .Get(profileUrl))
        {
            // TO DO: this should be asking for authorization on the header
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error getting profile with token " + token);
                Debug.Log($"{request.error}");
                yield break;
            }

            //TODO: check for errors even when the result is sucessful

            ProfileData profileData = JsonUtility.FromJson<ProfileData>(request.downloadHandler.text);
            string name = profileData.data.name;
            int fief = profileData.data.fief;

            PlayerPrefs.SetString("session_token", token);
            PlayerPrefs.Save();

            RequestExpeditionStatus();

            GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL
                .Invoke(profileData); //TODO: these 2 events here don't look good
            GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke(name, fief);
        }
    }

    IEnumerator GetLogout(string token)
    {
        string loginUrl = $"{baseUrl}{urlLogout}";
        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                GameManager.Instance.EVENT_REQUEST_LOGOUT_ERROR.Invoke(request.error);
                yield break;
            }

            LogoutData logoutData = JsonUtility.FromJson<LogoutData>(request.downloadHandler.text);
            string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result

            GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.Invoke(message);
        }
    }

    IEnumerator GetExpeditionStatus()
    {
        string token = PlayerPrefs.GetString("session_token");

        //Debug.Log("[RequestExpeditionStattus] with token " + token);

        string fullUrl = $"{baseUrl}{urlExpeditionStatus}";
        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            // using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
            request.SetRequestHeader("Accept", "*/*");
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            // Debug.Log(request.GetRequestHeader("Authorization"));

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error getting expedition status] " + request.error);

                yield break;
            }

            ExpeditionStatusData data = JsonUtility.FromJson<ExpeditionStatusData>(request.downloadHandler.text);

            GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(data.GetHasExpedition(), data.data.nftId);

            Debug.Log("[WebRequestManager] Expedition status " + request.downloadHandler.text);
        }
    }

    IEnumerator GetExpeditionScore()
    {
        string token = PlayerPrefs.GetString("session_token");

        string fullUrl = $"{baseUrl}{urlExpeditionScore}";

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("[Error getting expedition score] " + request.error);
                GameManager.Instance.EVENT_SHOW_SCOREBOARD.Invoke(null);
                yield break;
            }

            SWSM_ScoreboardData scoreboardData =
                JsonUtility.FromJson<SWSM_ScoreboardData>(request.downloadHandler.text);
            Debug.Log("answer from expedition score " + request.downloadHandler.text);
            GameManager.Instance.EVENT_SHOW_SCOREBOARD.Invoke(scoreboardData);
        }
    }

    IEnumerator GetCharacterList()
    {
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("[GetCharacterList] with token " + token);

        string fullUrl = $"{baseUrl}{urlCharactersList}";
        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Get($"{fullUrl}?Authorization={Uri.EscapeDataString(token)}"))
        {
            // using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error getting GetCharacterList status] " + request.error);

                yield break;
            }

            Debug.Log("answer from GetCharacterList status " + request.downloadHandler.text);


            //LogoutData answerData = JsonUtility.FromJson<LogoutData>(request.downloadHandler.text);
            //string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result
        }
    }

    public IEnumerator GetWalletContents(string walletAddress)
    {
        string fullUrl = $"{baseUrl}{urlWalletData}/{walletAddress}";
        int maxTry = 10;
        var tryDelay = new WaitForSeconds(3);

        UnityWebRequest request = null;


        for (int tryCount = 0; tryCount < maxTry; tryCount++)
        {
            Debug.Log($"[WebRequestManager] Getting Wallet Contents...");
            using (request = UnityWebRequest.Get($"{fullUrl}"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    yield return new WaitForEndOfFrame();
                    // moved this code inside the loop to encapsulate this in the using statement
                    Debug.Log($"[WebRequestManager] Wallet Contents Retrieved: {request?.downloadHandler.text}");
                    WalletKnightIds walletKnightIds =
                        JsonUtility.FromJson<WalletKnightIds>(request?.downloadHandler.text);
                    GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.Invoke(walletKnightIds);
                    yield break;
                }

                Debug.LogError($"[WebRequestManager] Error Getting Wallet Contents {request.error} from {fullUrl}");

                if (tryCount + 1 >= maxTry)
                {
                    Debug.Log($"[WebRequestManager] Will not try for wallet content again.");
                    GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke(
                        "ERROR: Could not gather wallet contents. Please try again later.", () => { });
                    yield break;
                }
                else
                {
                    Debug.Log($"[WebRequestManager] Retrying to get wallet contents...");
                    yield return tryDelay;
                }
            }
        }
    }


    public IEnumerator WhitelistStatus(float signRequest, string message, string signature, string wallet)
        {
            WWWForm form = new WWWForm();
            form.AddField("sig", signature); // The 0x signature string
            form.AddField("wallet", wallet); // The 0x wallet string
            form.AddField("created", (int)signRequest); // Unix Timestamp
            form.AddField("message", message); // String of what was signed
            string fullUrl = baseUrl + urlKoteWhitelist;
            using (UnityWebRequest request = UnityWebRequest.Post(fullUrl, form))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[WhitelistStatus] Error getting white list status: " + request.error);
                    yield break;
                }

                Debug.Log($"Whitelist status retrieved: {request.downloadHandler.text}");
                WhitelistResponse whitelistResponse =
                    JsonUtility.FromJson<WhitelistResponse>(request.downloadHandler.text);
                GameManager.Instance.EVENT_WHITELIST_CHECK_RECEIVED.Invoke(whitelistResponse.data.isValid);
            }
        }

        public IEnumerator GetNftData(int[] tokenIds)
        {
            List<int[]> splitTokenLists = new List<int[]>();
            for (int i = 0; i < tokenIds.Length; i += 30)
            {
                int[] tokenIdChunk;
                if (tokenIds.Length - i < 30)
                {
                    tokenIdChunk = new int[tokenIds.Length - i];
                }
                else
                {
                    tokenIdChunk = new int[30];
                }

                Array.Copy(tokenIds, i, tokenIdChunk, 0, tokenIdChunk.Length);
                splitTokenLists.Add(tokenIdChunk);
            }

            foreach (int[] idChunk in splitTokenLists)
            {
                UnityWebRequest openSeaRequest = NftRequest(idChunk);
                using (openSeaRequest)
                {
                    yield return openSeaRequest.SendWebRequest();
                    if (openSeaRequest.result == UnityWebRequest.Result.ConnectionError ||
                        openSeaRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.Log($"{openSeaRequest.error} {openSeaRequest.downloadHandler.text}");
                        yield break;
                    }

                    Debug.Log("Nft metadata received");
                    NftData nftData = JsonUtility.FromJson<NftData>(openSeaRequest.downloadHandler.text);
                    GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(nftData);
                }
            }
        }

        public UnityWebRequest NftRequest(int[] idChunk)
        {
            string nftUrl = urlOpenSea;
            nftUrl = nftUrl.Replace("xxxx", "token_ids=" + string.Join("&token_ids=", idChunk));
            Debug.Log("[WebRequesterManager] nft metadata url: " + nftUrl);
            UnityWebRequest openSeaRequest = UnityWebRequest.Get(nftUrl);
            openSeaRequest.SetRequestHeader("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
            return openSeaRequest;
        }

        public IEnumerator GetSingleNft(int tokenId)
        {
            UnityWebRequest openSeaRequest = NftRequest(new int[] { tokenId });
            yield return openSeaRequest.SendWebRequest();
            if (openSeaRequest.result == UnityWebRequest.Result.ConnectionError ||
                openSeaRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"{openSeaRequest.error} {openSeaRequest.downloadHandler.text}");
                yield break;
            }

            Debug.Log("Nft metadata received");
            NftData nftData = JsonUtility.FromJson<NftData>(openSeaRequest.downloadHandler.text);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(nftData);

            if (nftData.assets.Length > 0)
            {
                GameManager.Instance.EVENT_NFT_SELECTED.Invoke(nftData.assets[0]);
            }
            else
            {
                Debug.Log($"[WebRequesterManager] nft {tokenId} could not be found.");
            }
        }

        public IEnumerator GetNftImages()
        {
            nftQueueRunning = true;
            int requestsMade = 0;
            int requestsFailed = 0;
            int requestsSucceeded = 0;
            while (requestedNftImages.Count > 0)
            {
                (string, string) requestData = requestedNftImages.Dequeue();
                UnityWebRequest nftImageRequest = UnityWebRequestTexture.GetTexture(requestData.Item2);
                requestsMade++;
                yield return nftImageRequest.SendWebRequest();

                if (nftImageRequest.result == UnityWebRequest.Result.ConnectionError ||
                    nftImageRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(
                        $"[WebRequesterManager] [GetNftImages] Error getting nft image knight {requestData.Item1} at url {requestData.Item2}: {nftImageRequest.downloadHandler.text}");
                    requestsFailed++;
                    continue;
                }

                Texture2D myTexture = ((DownloadHandlerTexture)nftImageRequest.downloadHandler).texture;
                Sprite nftImage = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                    Vector2.zero);
                nftImage.name = requestData.Item1;
                GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.Invoke(requestData.Item1, nftImage);
                requestsSucceeded++;
            }

            nftQueueRunning = false;
            Debug.Log(
                $"[WebRequesterManager] [GetNftImages] requests made: {requestsMade} requests failed: {requestsFailed} requests succeeded: {requestsSucceeded}");
        }

        public IEnumerator GetNftSkinElement(TraitSprite spriteToPopulate)
        {
            string spriteName = spriteToPopulate.imageName + ".png";
            string spriteUrl = skinUrl + spriteName;

            using (UnityWebRequest nftSpriteRequest = UnityWebRequestTexture.GetTexture(spriteUrl))
            {
                yield return nftSpriteRequest.SendWebRequest();

                if (nftSpriteRequest.result == UnityWebRequest.Result.ConnectionError ||
                    nftSpriteRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(
                        $"Error getting nft skin sprite {spriteName} at url {spriteUrl} from server: {nftSpriteRequest.error}");
                    // keep track of failures so the player knows when to update
                    GameManager.Instance.EVENT_NFT_SKIN_SPRITE_FAILED.Invoke();
                    yield break;
                }

                Texture2D myTexture = ((DownloadHandlerTexture)nftSpriteRequest.downloadHandler).texture;
                Sprite nftSkinElement = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                    Vector2.zero);
                nftSkinElement.name = spriteName;
                spriteToPopulate.sprite = nftSkinElement;
                GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.Invoke(spriteToPopulate);
            }
        }

        public IEnumerator RequestNewExpedition(string characterType, string selectedNft)
        {
            string fullURL = $"{baseUrl}{urlExpeditionRequest}";

            string token = PlayerPrefs.GetString("session_token");

            WWWForm form = new WWWForm();
            form.AddField("class", characterType);
            form.AddField("nftId", selectedNft);

            using (UnityWebRequest request = UnityWebRequest.Post(fullURL, form))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Request new expedition error: " + $"{request.error}");
                    yield break;
                }

                ExpeditionRequestData data = JsonUtility.FromJson<ExpeditionRequestData>(request.downloadHandler.text);

                if (data.GetExpeditionStarted())
                {
                    Debug.Log("[RequestNewExpedition OK! ]");
                    GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
                }
                else
                {
                    Debug.Log("[Error on RequestNewExpedition]");
                }
            }
        }

        private IEnumerator CancelOngoingExpedition()
        {
            string fullURL = $"{baseUrl}{urlExpeditionCancel}";
            string token = PlayerPrefs.GetString("session_token");

            using (UnityWebRequest request = UnityWebRequest.Post(fullURL, ""))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("[Error canceling expedition]");
                    yield break;
                }

                GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(false, -1);
                Debug.Log("answer from cancel expedition " + request.downloadHandler.text);
            }
        }
    }