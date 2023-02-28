using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// Check HelperClasses.cs for the classes usaed to hold JSON data
/// </summary>
public class WebRequesterManager : SingleTon<WebRequesterManager>
{
    private string baseUrl => ClientEnvironmentManager.Instance.WebRequestURL;
    private string skinUrl => ClientEnvironmentManager.Instance.SkinURL;
    private string urlOpenSea => ClientEnvironmentManager.Instance.OpenSeasURL;

    // we have to queue the requested nft images due to rate limiting
    private Queue<(string, string)> requestedNftImages = new Queue<(string, string)>();
    private bool nftQueueRunning;

    private void Awake()
    {
        base.Awake();
        PlayerPrefs.SetString("api_url", baseUrl);

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
        GameManager.Instance.EVENT_SEND_BUG_FEEDBACK.AddListener(SendBugReport);
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

    public async UniTask<string> MakeRequest(UnityWebRequest request) 
    {
        await request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"{request.error}");
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Not Retrieved: {request.error}", CommunicationDirection.Incoming);
            return null;
        }
        else 
        {
            string rawJson = request.downloadHandler.text;
            ServerCommunicationLogger.Instance.LogCommunication($"[{request.uri}] Data Successfully Retrieved", CommunicationDirection.Incoming, rawJson);
            return rawJson;
        }
    }

    public IEnumerator GetRandomName(string lastName)
    {
        string randomNameUrl = $"{baseUrl}{RestEndpoint.RandomName}";

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
            JsonConvert.DeserializeObject<RandomNameData>(randomNameInfoRequest.downloadHandler.text);
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
        string registerUrl = $"{baseUrl}{RestEndpoint.Register}";
        WWWForm form = new WWWForm();
        form.AddField("name", nameText);
        form.AddField("email", email);
        form.AddField("email_confirmation", email);
        form.AddField("password", password);
        form.AddField("password_confirmation", password);

        ServerCommunicationLogger.Instance.LogCommunication(
            $"Registration requested. username: {nameText} email: {email}", CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Post(registerUrl, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Register error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication(
                $"Registration Success. {request.downloadHandler.text}", CommunicationDirection.Incoming);
            RegisterData registerData = JsonConvert.DeserializeObject<RegisterData>(request.downloadHandler.text);
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
        string loginUrl = $"{baseUrl}{RestEndpoint.Login}";

        Debug.Log("Login url:" + loginUrl);
        ServerCommunicationLogger.Instance.LogCommunication($"Login Requested. email: {email}",
            CommunicationDirection.Outgoing);
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Login error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                GameManager.Instance.EVENT_REQUEST_LOGIN_ERROR.Invoke(request.error);
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication($"Login success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
            LoginData loginData = JsonConvert.DeserializeObject<LoginData>(request.downloadHandler.text);
            string token = loginData.data.token;

            //TODO: check for errors even on successful result

            GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token);
        }
    }

    IEnumerator GetProfile(string token)
    {
        // Debug.Log("Getting profile with token " + token);

        string profileUrl = $"{baseUrl}{RestEndpoint.Profile}";
        ServerCommunicationLogger.Instance.LogCommunication("Profile request. token: " + token,
            CommunicationDirection.Outgoing);
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
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"Profile error. token: {token}, error: {request.error}",
                    CommunicationDirection.Incoming);
                Debug.Log($"{request.error}");
                yield break;
            }

            //TODO: check for errors even when the result is successful

            ServerCommunicationLogger.Instance.LogCommunication(
                $"Request profile successful: {request.downloadHandler.text}", CommunicationDirection.Incoming);
            ProfileData profileData = JsonConvert.DeserializeObject<ProfileData>(request.downloadHandler.text);
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
        string loginUrl = $"{baseUrl}{RestEndpoint.Logout}";
        WWWForm form = new WWWForm();

        ServerCommunicationLogger.Instance.LogCommunication($"Logout request. token: {token}",
            CommunicationDirection.Outgoing);
        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                ServerCommunicationLogger.Instance.LogCommunication($"Logout request error: {request.error}",
                    CommunicationDirection.Incoming);
                GameManager.Instance.EVENT_REQUEST_LOGOUT_ERROR.Invoke(request.error);
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication($"Logout Success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
            LogoutData logoutData = JsonConvert.DeserializeObject<LogoutData>(request.downloadHandler.text);
            string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result

            GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.Invoke(message);
        }
    }

    IEnumerator GetExpeditionStatus()
    {
        string token = PlayerPrefs.GetString("session_token");

        //Debug.Log("[RequestExpeditionStattus] with token " + token);

        string fullUrl = $"{baseUrl}{RestEndpoint.ExpeditionStatus}";
        WWWForm form = new WWWForm();
        ServerCommunicationLogger.Instance.LogCommunication($"Expedition Status request. token: {token}",
            CommunicationDirection.Outgoing);
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
                ServerCommunicationLogger.Instance.LogCommunication($"Expedition status error: {request.error}",
                    CommunicationDirection.Incoming);

                Debug.Log("[Error getting expedition status] " + request.error);

                yield break;
            }

            ExpeditionStatusData data = JsonConvert.DeserializeObject<ExpeditionStatusData>(request.downloadHandler.text);

            GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(data.GetHasExpedition(), data.data.nftId);

            Debug.Log("[WebRequestManager] Expedition status " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                $"Expedition status success: {request.downloadHandler.text}",
                CommunicationDirection.Incoming);
        }
    }

    IEnumerator GetExpeditionScore()
    {
        string token = PlayerPrefs.GetString("session_token");

        string fullUrl = $"{baseUrl}{RestEndpoint.ExpeditionScore}";

        ServerCommunicationLogger.Instance.LogCommunication($"Expedition score request. token: {token}",
            CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("[Error getting expedition score] " + request.error);
                ServerCommunicationLogger.Instance.LogCommunication($"Expedition score error: {request.error}",
                    CommunicationDirection.Incoming);

                GameManager.Instance.EVENT_SHOW_SCOREBOARD.Invoke(null);
                yield break;
            }

            SWSM_ScoreboardData scoreboardData =
                JsonConvert.DeserializeObject<SWSM_ScoreboardData>(request.downloadHandler.text);
            Debug.Log("answer from expedition score " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "Expedition score success: " + request.downloadHandler.text, CommunicationDirection.Incoming);
            GameManager.Instance.EVENT_SHOW_SCOREBOARD.Invoke(scoreboardData);
        }
    }

    IEnumerator GetCharacterList()
    {
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("[GetCharacterList] with token " + token);
        ServerCommunicationLogger.Instance.LogCommunication("Character list request. token: " + token,
            CommunicationDirection.Outgoing);

        string fullUrl = $"{baseUrl}{RestEndpoint.CharactersList}";
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
                ServerCommunicationLogger.Instance.LogCommunication(
                    "Character list error: " + request.error, CommunicationDirection.Incoming);

                yield break;
            }

            Debug.Log("answer from GetCharacterList status " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "Character list success: " + request.downloadHandler.text, CommunicationDirection.Incoming);


            //LogoutData answerData = JsonConvert.DeserializeObject<LogoutData>(request.downloadHandler.text);
            //string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result
        }
    }

    public IEnumerator GetWalletContents(string walletAddress)
    {
        string fullUrl = $"{baseUrl}{RestEndpoint.WalletData}/{walletAddress}";
        int maxTry = 10;
        var tryDelay = new WaitForSeconds(3);

        UnityWebRequest request = null;


        for (int tryCount = 0; tryCount < maxTry; tryCount++)
        {
            Debug.Log($"[WebRequestManager] Getting Wallet Contents...");
            ServerCommunicationLogger.Instance.LogCommunication(
                $"Wallet content request: wallet {walletAddress}",
                CommunicationDirection.Outgoing);
            using (request = UnityWebRequest.Get($"{fullUrl}"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    yield return new WaitForEndOfFrame();
                    // moved this code inside the loop to encapsulate this in the using statement
                    Debug.Log($"[WebRequestManager] Wallet Contents Retrieved: {request?.downloadHandler.text}");
                    ServerCommunicationLogger.Instance.LogCommunication(
                        $"Wallet Content success: {request.downloadHandler.text}",
                        CommunicationDirection.Incoming);
                    WalletKnightIds walletKnightIds =
                        JsonConvert.DeserializeObject<WalletKnightIds>(request.downloadHandler.text);
                    GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.Invoke(walletKnightIds);
                    yield break;
                }

                Debug.LogError($"[WebRequestManager] Error Getting Wallet Contents {request.error} from {fullUrl}");
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"Wallet content error: {request.error}",
                    CommunicationDirection.Incoming);

                if (tryCount + 1 >= maxTry)
                {
                    Debug.Log($"[WebRequestManager] Will not try for wallet content again.");
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
        string fullUrl = baseUrl + RestEndpoint.KoteWhitelist;
        ServerCommunicationLogger.Instance.LogCommunication(
            $"Whitelist status request: {form}",
            CommunicationDirection.Outgoing);
        using (UnityWebRequest request = UnityWebRequest.Post(fullUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("[WhitelistStatus] Error getting white list status: " + request.error);
                ServerCommunicationLogger.Instance.LogCommunication(
                    "Whitelist error: " + request.error,
                    CommunicationDirection.Incoming);
                yield break;
            }

            Debug.Log($"Whitelist status retrieved: {request.downloadHandler.text}");
            ServerCommunicationLogger.Instance.LogCommunication(
                $"Whitelist success: {request.downloadHandler.text}", CommunicationDirection.Incoming);
            WhitelistResponse whitelistResponse =
                JsonConvert.DeserializeObject<WhitelistResponse>(request.downloadHandler.text);
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
            ServerCommunicationLogger.Instance.LogCommunication($"Metadata request: {openSeaRequest.url}",
                CommunicationDirection.Outgoing);
            using (openSeaRequest)
            {
                yield return openSeaRequest.SendWebRequest();
                if (openSeaRequest.result == UnityWebRequest.Result.ConnectionError ||
                    openSeaRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"{openSeaRequest.error} {openSeaRequest.downloadHandler.text}");
                    ServerCommunicationLogger.Instance.LogCommunication(
                        $"Metadata error: {openSeaRequest.error} {openSeaRequest.downloadHandler.text}",
                        CommunicationDirection.Incoming);
                    yield break;
                }

                Debug.Log("Nft metadata received");
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"Metadata success: {openSeaRequest.downloadHandler.text}", CommunicationDirection.Incoming);
                NftData nftData = JsonConvert.DeserializeObject<NftData>(openSeaRequest.downloadHandler.text);
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
        using (UnityWebRequest openSeaRequest = NftRequest(new int[] { tokenId }))
        {
            yield return openSeaRequest.SendWebRequest();
            if (openSeaRequest.result == UnityWebRequest.Result.ConnectionError ||
                openSeaRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"{openSeaRequest.error} {openSeaRequest.downloadHandler.text}");
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"Single metadata error: {openSeaRequest.error} {openSeaRequest.downloadHandler.text}",
                    CommunicationDirection.Incoming);
                yield break;
            }

            Debug.Log("Nft metadata received");
            ServerCommunicationLogger.Instance.LogCommunication(
                $"Single metadata success: {openSeaRequest.downloadHandler.text}", CommunicationDirection.Incoming);
            NftData nftData = JsonConvert.DeserializeObject<NftData>(openSeaRequest.downloadHandler.text);
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
            ServerCommunicationLogger.Instance.LogCommunication($"nft image request. nftId: {requestData.Item1}",
                CommunicationDirection.Outgoing);

            yield return nftImageRequest.SendWebRequest();

            if (nftImageRequest.result == UnityWebRequest.Result.ConnectionError ||
                nftImageRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(
                    $"[WebRequesterManager] [GetNftImages] Error getting nft image knight {requestData.Item1} at url {requestData.Item2}: {nftImageRequest.downloadHandler.text}");
                ServerCommunicationLogger.Instance.LogCommunication(
                    $"nft image error: {nftImageRequest.error} {nftImageRequest.downloadHandler.text}",
                    CommunicationDirection.Incoming);
                requestsFailed++;
                continue;
            }

            Texture2D myTexture = ((DownloadHandlerTexture)nftImageRequest.downloadHandler).texture;
            Sprite nftImage = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                Vector2.zero);
            nftImage.name = requestData.Item1;
            ServerCommunicationLogger.Instance.LogCommunication($"nft image success. nftId: {requestData.Item1}",
                CommunicationDirection.Incoming);
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

        ServerCommunicationLogger.Instance.LogCommunication(
            $"Skin image request: sprite: {spriteName} url: {spriteUrl}",
            CommunicationDirection.Outgoing);
        using (UnityWebRequest nftSpriteRequest = UnityWebRequestTexture.GetTexture(spriteUrl))
        {
            yield return nftSpriteRequest.SendWebRequest();

            if (nftSpriteRequest.result == UnityWebRequest.Result.ConnectionError ||
                nftSpriteRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(
                    $"Error getting nft skin sprite {spriteName} at url {spriteUrl} from server: {nftSpriteRequest.error}");
                ServerCommunicationLogger.Instance.LogCommunication($"Skin image error: {nftSpriteRequest.error}",
                    CommunicationDirection.Incoming);
                // keep track of failures so the player knows when to update
                GameManager.Instance.EVENT_NFT_SKIN_SPRITE_FAILED.Invoke();
                yield break;
            }

            Texture2D myTexture = ((DownloadHandlerTexture)nftSpriteRequest.downloadHandler).texture;
            Sprite nftSkinElement = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height),
                Vector2.zero);
            nftSkinElement.name = spriteName;
            spriteToPopulate.sprite = nftSkinElement;
            ServerCommunicationLogger.Instance.LogCommunication($"Skin image success. image: {spriteName}",
                CommunicationDirection.Incoming);
            GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.Invoke(spriteToPopulate);
        }
    }

    public IEnumerator RequestNewExpedition(string characterType, string selectedNft)
    {
        string fullURL = $"{baseUrl}{RestEndpoint.ExpeditionRequest}";

        string token = PlayerPrefs.GetString("session_token");

        WWWForm form = new WWWForm();
        form.AddField("class", characterType);
        form.AddField("nftId", selectedNft);
        ServerCommunicationLogger.Instance.LogCommunication($"New expedition request: {form}",
            CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Post(fullURL, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Request new expedition error: " + $"{request.error}");
                ServerCommunicationLogger.Instance.LogCommunication($"new expedition error: {request.error}",
                    CommunicationDirection.Incoming);
                yield break;
            }

            ServerCommunicationLogger.Instance.LogCommunication(
                $"New expedition success: {request.downloadHandler.text}", CommunicationDirection.Incoming);
            ExpeditionRequestData data = JsonConvert.DeserializeObject<ExpeditionRequestData>(request.downloadHandler.text);

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
        string fullURL = $"{baseUrl}{RestEndpoint.ExpeditionCancel}";
        string token = PlayerPrefs.GetString("session_token");

        ServerCommunicationLogger.Instance.LogCommunication($"Expedition cancel request. token: {token}",
            CommunicationDirection.Outgoing);

        using (UnityWebRequest request = UnityWebRequest.Post(fullURL, ""))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error canceling expedition]");
                ServerCommunicationLogger.Instance.LogCommunication("Expedition cancel error: " + request.error,
                    CommunicationDirection.Outgoing);
                yield break;
            }

            GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(false, -1);
            Debug.Log("answer from cancel expedition " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "cancel expedition success: " + request.downloadHandler.text, CommunicationDirection.Outgoing);
        }
    }

    private async void SendBugReport(string title, string description, string base64Image)
    {
        string fullUrl = $"{ClientEnvironmentManager.Instance.WebSocketURL}{RestEndpoint.BugReport}";

        BugReportData reportData = new BugReportData
        {
            reportId = Guid.NewGuid().ToString(),
            environment = ClientEnvironmentManager.Instance.Environment.ToString(),
            clientId = UserDataManager.Instance.ClientId,
            account = UserDataManager.Instance.UserAccount,
            knightId = UserDataManager.Instance.ActiveNft,
            expeditionId = UserDataManager.Instance.ExpeditionId,
            userTitle = title,
            userDescription = description,
            screenshot = base64Image,
            frontendVersion = VersionManager.ClientVersionWithCommit,
            backendVersion = VersionManager.ServerVersion,
            messageLog = ServerCommunicationLogger.Instance.GetCommunicationLog()
        };
        string data = JsonConvert.SerializeObject(reportData);
        Debug.Log(data);
        byte[] utf8String = Encoding.Default.GetBytes(data);
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            var uploadHandler = new UploadHandlerRaw(utf8String);
            uploadHandler.contentType = $"application/json";
            request.uploadHandler = uploadHandler;
            await MakeRequest(request);
            uploadHandler.Dispose();
        }
    }

    public string ConstructUrl(string path) 
    {
        string host = ClientEnvironmentManager.Instance.WebRequestURL;
        return $"{host}{path}";
    }
}

public static class RestEndpoint 
{
    public static readonly string RandomName = "/auth/v1/generate/username";
    public static readonly string Register = "/auth/v1/register";
    public static readonly string Login = "/auth/v1/login";
    public static readonly string Logout = "/auth/v1/logout";
    public static readonly string Profile = "/gsrv/v1/profile";
    public static readonly string WalletData = "/gsrv/v1/wallets";
    public static readonly string KoteWhitelist = "/gsrv/v1/tokens/verify";
    public static readonly string CharactersList = "/gsrv/v1/characters";
    public static readonly string ExpeditionStatus = "/gsrv/v1/expeditions/status";
    public static readonly string ExpeditionRequest = "/gsrv/v1/expeditions";
    public static readonly string ExpeditionCancel = "/gsrv/v1/expeditions/cancel";
    public static readonly string ExpeditionScore = "/gsrv/v1/expeditions/score";
    public static readonly string BugReport = "/v1/bugReports";
    public static readonly string ServerVersion = "/gsrv/v1/showversion";
}