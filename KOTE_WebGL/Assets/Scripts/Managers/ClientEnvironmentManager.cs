using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ClientEnvironmentManager : ISingleton<ClientEnvironmentManager>
{
    private static ClientEnvironmentManager instance;

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

    // this is the first thing called when the game starts
    public async UniTask StartEnvironmentManger()
    {
#if UNITY_WEBGL
        using (UnityWebRequest request =
               UnityWebRequest.Get($"{Application.absoluteURL}environment.json"))
        {
            try
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetEnvironmentData(null);
                    return;
                }

                try
                {
                    EnvironmentUrls environmentUrls =
                        JsonConvert.DeserializeObject<EnvironmentUrls>(request.downloadHandler.text);
                    SetEnvironmentData(environmentUrls);
                }
                catch (ArgumentNullException ex)
                {
                    SetEnvironmentData(null);
                }
            }
            catch (UnityWebRequestException e)
            {
                SetEnvironmentData(null);
            }
        }
#endif
    }


#if UNITY_EDITOR
    public bool InUnity = false;
    public UnityEnvironment UnityEnvironment =>
        AssetDatabase.LoadAssetAtPath<UnityEnvironment>("Assets/UnityEnvironment.asset");
#endif
    public string WebRequestURL => _environmentUrls.WebRequestURL;
    public string SkinURL => _environmentUrls.SkinURL;
    public string GearIconURL => _environmentUrls.GearIconURL;
    public string PortraitElementURL => _environmentUrls.PortraitElementURL;
    public string WebSocketURL => _environmentUrls.WebSocketURL;

    private EnvironmentUrls _environmentUrls;
    public Environments Environment { get; private set; } = Environments.Unknown;

    private ClientEnvironmentManager()
    {
#if UNITY_EDITOR
        InUnity = true;
        Environment = DetermineEnvironment(Application.absoluteURL);
        UpdateUrls(Environment);
#endif
    }

    public void SetEnvironmentData(EnvironmentUrls urls)
    {
        Environment = DetermineEnvironment(Application.absoluteURL);

        if (urls != null && urls.DoAllUrlsExist())
        {
            _environmentUrls = urls;
            return;
        }

        UpdateUrls(Environment);
    }

    public Environments DetermineEnvironment(string applicationURL)
    {
        string hostName = applicationURL.ToLower();
#if UNITY_EDITOR
        if (InUnity == true)
        {
            return Environments.Unity;
        }
#endif
        if (hostName.Contains("villager") || hostName.Contains("snapshot"))
        {
            return Environments.Snapshot;
        }
        else if (hostName.Contains("dev"))
        {
            return Environments.Dev;
        }
        else if (hostName.Contains("stage"))
        {
            return Environments.Stage;
        }
        else if (hostName.Contains("alpha") && hostName.Contains("robotseamonster"))
        {
            return Environments.TestAlpha;
        }
        else if (hostName.Contains("alpha"))
        {
            return Environments.Alpha;
        }

        return Environments.Unknown;
    }

    private void UpdateUrls(Environments currentEnvironment)
    {
        _environmentUrls = new EnvironmentUrls
        {
            WebRequestURL = $"http://localhost:3000",
            WebSocketURL = $"http://localhost:3000"
        };
        return;
        switch (currentEnvironment)
        {
            case Environments.Dev:
            // unknown usually means a local build
            case Environments.Unknown:
            default:
                _environmentUrls = new EnvironmentUrls
                {
                    WebRequestURL = $"https://gateway.dev.kote.robotseamonster.com",
                    WebSocketURL = $"https://api.dev.kote.robotseamonster.com"
                };
                break;
            case Environments.Snapshot:
                _environmentUrls = new EnvironmentUrls
                {
                    WebRequestURL = $"https://gateway.villagers.dev.kote.robotseamonster.com",
                    WebSocketURL = $"https://api.villagers.dev.kote.robotseamonster.com"
                };
                break;
            case Environments.Stage:
                _environmentUrls = new EnvironmentUrls
                {
                    WebRequestURL = $"https://gateway.stage.kote.robotseamonster.com",
                };
                break;
            case Environments.TestAlpha:
                _environmentUrls = new EnvironmentUrls
                {
                    WebRequestURL = $"https://gateway.alpha.kote.robotseamonster.com",
                    WebSocketURL = $"https://api.alpha.kote.robotseamonster.com"
                };
                break;
            case Environments.Alpha:
                _environmentUrls = new EnvironmentUrls
                {
                    WebRequestURL = $"https://gateway.alpha.knightsoftheether.com",
                    WebSocketURL = $"https://api.alpha.knightsoftheether.com"
                };
                break;
#if UNITY_EDITOR
            case Environments.Unity:
                Environments emulate = UnityEnvironment.EnvironmentToEmulate;
                if (emulate == Environments.Unity)
                    emulate = Environments.Dev;
                UpdateUrls(emulate);
                break;
#endif
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