using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEnvironmentManager: ISingleton<ClientEnvironmentManager>
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

#if UNITY_EDITOR
    public bool InUnity = false;
#endif
    public string WebRequestURL { get; private set; }
    public string SkinURL { get; private set; }
    public string WebSocketURL { get; private set; }
    public string OpenSeasURL { get; private set; } =
        "https://api.opensea.io/api/v1/assets?xxxx&asset_contract_address=0x32A322C7C77840c383961B8aB503c9f45440c81f&format=json";
    public Environments Environment { get; private set; } = Environments.Unknown;

    private ClientEnvironmentManager()
    {
#if UNITY_EDITOR
        InUnity = true;
#endif
        Environment = DetermineEnvironment(Application.absoluteURL);
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
        switch(currentEnvironment) 
        {
            case Environments.Stage:
                WebRequestURL = $"https://gateway.stage.kote.robotseamonster.com";
                SkinURL = $"https://koteskins.robotseamonster.com/";
                WebSocketURL = $"https://api.stage.kote.robotseamonster.com";
                break;
            case Environments.TestAlpha:
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
        Dev,
        Stage,
        TestAlpha,
        Alpha
    }
}
