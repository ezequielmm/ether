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

    public async Task StartEnvironmentManger(bool localEnvironment)
    {
        var url = "https://dev.knightsoftheether.com:8081/environment.json";
#if UNITY_EDITOR
        if (localEnvironment)
            url = $"{Application.streamingAssetsPath}/environment.json";
#endif

        using UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(request.downloadHandler.text);
            string jsonText = Encoding.UTF8.GetString(utf8Bytes);
            
            var deserializeObject = JsonConvert.DeserializeObject<EnvironmentUrls>(jsonText);

            _environmentUrls = deserializeObject;
        }
        else
        {
            Debug.LogError($"environment error: {request.error}");
        }
    }
}

[Serializable]
public class EnvironmentUrls
{
    [JsonProperty("request_url")] public string WebRequestURL;
    [JsonProperty("socket_url")] public string WebSocketURL;
    [JsonProperty("asset_url")] public string ArtAssetURL;
    
    private string AssetsUrl;
    public EnvironmentUrls()
    {
        AssetsUrl = (string.IsNullOrEmpty(ArtAssetURL)) ? Application.streamingAssetsPath : ArtAssetURL;
        #if UNITY_EDITOR || DEVELOPMENT_BUILD || !UNITY_WEBGL
            AssetsUrl =  $"https://koteskins.robotseamonster.com/";
        #endif
    }

    [JsonIgnore]
    public string SkinURL => AssetsUrl.AddPath("SkinSprites");

    [JsonIgnore]
    public string GearIconURL => AssetsUrl.AddPath("GearIcons");

    [JsonIgnore]
    public string PortraitElementURL => AssetsUrl.AddPath("Portraits");


    public bool DoAllUrlsExist()
    {
        bool allExist = true;
        if (string.IsNullOrEmpty(WebRequestURL)) allExist = false;
        if (string.IsNullOrEmpty(WebSocketURL)) allExist = false;

        return allExist;
    }
}