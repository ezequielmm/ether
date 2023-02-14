using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEnvironmentManager
{
    private static ClientEnvironmentManager _instance;
    public static ClientEnvironmentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ClientEnvironmentManager();
            }
            return _instance;
        }
    }

    public string WebRequestURL { get; private set; }
    public string SkinURL { get; private set; }
    public string WebSocketURL { get; private set; }
    public string OpenSeasURL { get; private set; } =
        "https://api.opensea.io/api/v1/assets?xxxx&asset_contract_address=0x32A322C7C77840c383961B8aB503c9f45440c81f&format=json";
    private Environments environment = Environments.Unknown;
    public string EnvironmentName => environment.ToString();

    private ClientEnvironmentManager()
    {
        environment = DetermineEnvironment(Application.absoluteURL);
        UpdateUrls(environment);
        Debug.Log($"Now running on Environment: {EnvironmentName}");
    }

    private Environments DetermineEnvironment(string applicationURL)
    {
        string hostName = applicationURL.ToLower();

#if UNITY_EDITOR
        return Environments.Unity;
#endif
        if (hostName.Contains("dev"))
        {
            return Environments.Dev;
        }
        else if (hostName.Contains("stage"))
        {
            return Environments.Stage;
        }
        else if (hostName.Contains("alpha") && hostName.Contains("robotseamonster"))
        {
            return Environments.AlphaTest;
        }
        else if (hostName.Contains("alpha"))
        {
            return Environments.Alpha;
        }
        return Environments.Unknown;
    }

    private void UpdateUrls(Environments currentEnvironment) 
    {
        switch(currentEnvironment) 
        {

            case Environments.Stage:
                WebRequestURL = $"https://gateway.stage.kote.robotseamonster.com";
                SkinURL = $"https://koteskins.robotseamonster.com/";
                WebSocketURL = $"https://api.stage.kote.robotseamonster.com";
                break;
            case Environments.AlphaTest:
                WebRequestURL = $"https://gateway.alpha.kote.robotseamonster.com";
                SkinURL = $"https://koteskins.robotseamonster.com/";
                WebSocketURL = $"https://api.alpha.kote.robotseamonster.com";
                break;
            case Environments.Alpha:
                WebRequestURL = $"https://gateway.alpha.knightsoftheether.com";
                SkinURL = $"https://s3.amazonaws.com/koteskins.knightsoftheether.com/";
                WebSocketURL = $"https://api.alpha.knightsoftheether.com:443";
                break;
#if UNITY_EDITOR
            case Environments.Unity:
#endif
            case Environments.Unknown:
            case Environments.Dev:
            default:
                WebRequestURL = $"https://gateway.dev.kote.robotseamonster.com";
                SkinURL = $"https://koteskins.robotseamonster.com/";
                WebSocketURL = $"https://api.dev.kote.robotseamonster.com";
                break;
        }
    }

    private enum Environments 
    {
        Unknown,
#if UNITY_EDITOR
        Unity,
#endif
        Dev,
        Stage,
        AlphaTest,
        Alpha
    }
}
