using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

public static class OpenSeasRequstBuilder
{
    public static int MaxContentRequest => GameSettings.MAX_OPENSEA_CONTENT_REQUEST;
    public static UnityWebRequest ConstructTokenRequest(string contractId, params int[] tokenIds)
    {
        string requestUrl = BuildRequestUrl(contractId, tokenIds);
        UnityWebRequest openSeaRequest = UnityWebRequest.Get(requestUrl);
        openSeaRequest.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        return openSeaRequest;
    }
    public static UnityWebRequest ConstructNftRequest(NftContract contract, params int[] tokenIds)
    {
        string KnightContract = NftManager.GetNftContractAddress(contract);
        return ConstructTokenRequest(KnightContract, tokenIds);
    }
    private static string BuildRequestUrl(string contractId, params int[] tokenIds)
    {
        StringBuilder requestString = new StringBuilder();
        if (!NftManager.IsTestNet)
        {
            requestString.Append("https://api.opensea.io/api/v1/assets?token_ids=");
        }

        if (NftManager.IsTestNet)
        {
            requestString.Append("https://testnets-api.opensea.io/api/v1/assets?token_ids=");
        }
        requestString.Append(string.Join("&token_ids=", tokenIds));
        requestString.Append("&limit=");
        requestString.Append(MaxContentRequest);
        requestString.Append("&format=json&asset_contract_address=");
        requestString.Append(contractId);
        return requestString.ToString();
    }
}
