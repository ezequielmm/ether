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
            string rawJson = await webRequest.MakeRequest(request);
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

    public async UniTask<List<NftMetaData>> GetNFTData(List<int> knightIds)
    {
        var requestBatch = knightIds.Partition(OpenSeasRequstBuilder.MaxContentRequest);
        List<NftMetaData> nftMetaDataList = new List<NftMetaData>();
        foreach (var knightId in knightIds)
        {
            using (UnityWebRequest request = OpenSeasRequstBuilder.ConstructKnightRequest(knightIds.ToArray()))
            {
                string rawJson = await webRequest.MakeRequest(request);
                nftMetaDataList.AddRange(ParseJsonWithPath<List<NftMetaData>>(rawJson, "assets"));
            }
        }
        return nftMetaDataList;
    }

    public async UniTask<bool> VerifyWallet(WalletSignature walletSignature)
    {
        WWWForm form = walletSignature.ToWWWForm();
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.VerifyWalletSignature);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string rawJson = await webRequest.MakeRequest(request);
            return ParseJsonWithPath<bool>(rawJson, "data.isValid");
        }
    }

    public async UniTask<List<int>> GetNftsInWalletPerContract(string wallet, string contract) 
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.VerifyWalletSignature) + $"/{wallet}";
        using(UnityWebRequest request = UnityWebRequest.Get(requestUrl)) 
        {
            string rawJson = await KeepRetryingRequest(request);
            return ParseJsonWithPath<List<int>>(rawJson, "data");
        }
        
    }

    private async UniTask<string> KeepRetryingRequest(UnityWebRequest request, int tryLimit = 10, float retryDelaySeconds = 3) 
    {
        bool successful = false;
        int trys = 0;
        int retryDelayInMiliseconds = Mathf.RoundToInt(retryDelaySeconds * 1000);
        using (request) 
        {
            string rawJson = null;
            while (!successful && trys < tryLimit)
            {
                rawJson = await webRequest.MakeRequest(request);
                successful = !string.IsNullOrEmpty(rawJson);
                trys++;
            }
            await UniTask.Delay(retryDelayInMiliseconds);
            return rawJson;
        }
    }

    public static T ParseJsonWithPath<T>(string rawJson, string tokenPath) 
    {
        if (string.IsNullOrEmpty(rawJson)) 
        {
            return default(T);
        }
        JObject json = JObject.Parse(rawJson);
        T data = json.SelectToken(tokenPath).ToObject<T>();
        return data;
    }
}
