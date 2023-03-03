using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.VerifyWalletSignature) + $"/{wallet}";
        Debug.Log(requestUrl);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl)) 
        {
            string rawJson = await KeepRetryingRequest(request);
            Debug.Log(rawJson);
            return ParseJsonWithPath<List<int>>(rawJson, "data");
        }
    }
    public async UniTask<List<Nft>> GetNftMetaData(List<int> tokenId, NftContract contract)
    {
        var requestBatch = tokenId.Partition(OpenSeasRequstBuilder.MaxContentRequest);
        List<Nft> nftMetaDataList = new List<Nft>();
        foreach (var knightId in tokenId)
        {
            using (UnityWebRequest request = OpenSeasRequstBuilder.ConstructNftRequest(contract, tokenId.ToArray()))
            {
                string rawJson = await MakeJsonRequest(request);
                nftMetaDataList.AddRange(ParseJsonWithPath<List<Nft>>(rawJson, "assets"));
            }
        }
        foreach (var token in nftMetaDataList) 
        {
            token.Contract = contract;
        }
        return nftMetaDataList;
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

    public async UniTask<bool> RequestNewExpedition(string characterType, int selectedNft)
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ExpeditionRequest);
        
        WWWForm form = new WWWForm();
        form.AddField("class", characterType);
        form.AddField("nftId", selectedNft);

        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            request.AddAuthToken();
            string rawJson = await MakeJsonRequest(request);
            return ParseJsonWithPath<bool>(rawJson, "data.expeditionCreated");
        }
    }


    private async UniTask<string> KeepRetryingRequest(UnityWebRequest request, int tryLimit = 10, float retryDelaySeconds = 3) 
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
        return (await webRequest.MakeRequest(request))?.text;
    }

    private async UniTask<Texture2D> MakeTextureRequest(UnityWebRequest request)
    {
        return ((DownloadHandlerTexture)await webRequest.MakeRequest(request))?.texture;
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
