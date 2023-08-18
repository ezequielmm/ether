using System;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ClientEnvironmentManager : ISingleton<ClientEnvironmentManager>
{
    private static ClientEnvironmentManager instance;
    
    public string WebRequestURL => _environmentUrls.WebRequestURL;
    public string SkinURL => _environmentUrls.SkinURL;
    public string GearIconURL => _environmentUrls.GearIconURL;
    public string PortraitElementURL => _environmentUrls.PortraitElementURL;
    public string WebSocketURL => _environmentUrls.WebSocketURL;
    public string LeaderboardURL => _environmentUrls.LeaderboardURL;
    
    public string LootboxAssetsURL => _environmentUrls.LootboxAssetsURL;

    private EnvironmentUrls _environmentUrls;
    public Environments Environment { get; private set; } = Environments.Unknown;
    
    public static ClientEnvironmentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ClientEnvironmentManager();
            }

            return instance;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }

    public enum Environments
    {
        Unknown,
#if UNITY_EDITOR
        Unity,
#endif
        Snapshot,
        Dev,
        Stage,
        TestAlpha,
        Alpha
    }

    public async Task StartEnvironmentManger()
    {
        Debug.Log("host " + URLParameters.Host);
        Debug.Log("origin " + URLParameters.Origin);
        var host = GetStreamingAssetsPath();
        string path = "/environment.json";
        string url = host + path;
 #if !UNITY_EDITOR
        Debug.Log("Get env from remote host " + url);
 #else
        Debug.Log("Get env from local-editor host " + url);
 #endif

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(request.downloadHandler.text);
            string jsonText = Encoding.UTF8.GetString(utf8Bytes);
            
            var deserializeObject = JsonConvert.DeserializeObject<EnvironmentUrls>(jsonText);

            _environmentUrls = deserializeObject;

            Debug.Log("got env!! " + _environmentUrls.WebRequestURL);
        }
        else
        {
            Debug.LogError($"environment error: {request.error}");
        }
    }

    public static string GetStreamingAssetsPath()
    {
#if !UNITY_EDITOR
        return $"{URLParameters.Origin}";
#else
        return $"{Application.streamingAssetsPath}";
#endif
    }
}

[Serializable]
public class EnvironmentUrls
{
    [JsonProperty("request_url")] public string WebRequestURL;
    [JsonProperty("socket_url")] public string WebSocketURL;
    [JsonProperty("asset_url")] public string ArtAssetURL;
    [JsonProperty("leaderboard_url")] public string LeaderboardURL;

    private string AssetsUrl;
    public EnvironmentUrls()
    {
        AssetsUrl = (string.IsNullOrEmpty(ArtAssetURL)) ? Application.streamingAssetsPath : ArtAssetURL;
    }

    [JsonIgnore]
    public string SkinURL => AssetsUrl.AddPath("SkinSprites");

    [JsonIgnore]
    public string GearIconURL => AssetsUrl.AddPath("GearIcons");

    [JsonIgnore]
    public string PortraitElementURL => AssetsUrl.AddPath("Portraits");

    [JsonIgnore]
    public string LootboxAssetsURL => AssetsUrl.AddPath("LootboxAssets");

    public bool DoAllUrlsExist()
    {
        bool allExist = true;
        if (string.IsNullOrEmpty(WebRequestURL)) allExist = false;
        if (string.IsNullOrEmpty(WebSocketURL)) allExist = false;

        return allExist;
    }
}