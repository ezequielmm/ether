using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

/// <summary>
/// Check HelperClasses.cs for the classes usaed to hold JSON data
/// </summary>
public class WebRequesterManager : MonoBehaviour
{
    private readonly string baseUrl = "https://gateway.kote.robotseamonster.com";
    private readonly string urlRandomName = "/auth/v1/generate/username";
    private readonly string urlRegister = "/auth/v1/register";
    private readonly string urlLogin = "/auth/v1/login";
    private readonly string urlProfile = "/gsrv/v1/profile";
    private readonly string urlLogout = "/auth/v1/logout";
    private readonly string urlExpeditionStatus = "/gsrv/v1/expeditions/status";
    private readonly string urlCharactersList = "/gsrv/v1/characters";
    private readonly string urlExpeditionRequest = "/gsrv/v1/expeditions";
    private readonly string urlExpeditionCancel = "/gsrv/v1/expeditions/cancel";


    private void Awake()
    {
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();

        GameManager.Instance.EVENT_REQUEST_NAME.AddListener(OnRandomNameEvent);
        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener(OnRegisterEvent);
        GameManager.Instance.EVENT_REQUEST_LOGIN.AddListener(RequestLogin);
        GameManager.Instance.EVENT_REQUEST_PROFILE.AddListener(RequestProfile);
        GameManager.Instance.EVENT_REQUEST_LOGOUT.AddListener(RequestLogout);
        GameManager.Instance.EVENT_REQUEST_EXPEDITION_CANCEL.AddListener(RequestExpeditionCancel);
    }

    private void Start()
    {
        GameManager.Instance.webRequester = this;
        DontDestroyOnLoad(this);      
    }

    internal void RequestStartExpedition(string characterType)
    {
        StartCoroutine(RequestNewExpedition(characterType));
    }

    public void RequestLogout(string token)
    {
        StartCoroutine(GetLogout(token));
    }

    public void RequestExpeditionStatus()
    {

        StartCoroutine(GetExpeditionStatus());
    }

    public void RequestExpeditionCancel()
    {
        StartCoroutine(CancelOngoingExpedition());
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

    public void RequestCharacterList()
    {
        StartCoroutine(GetCharacterList());
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

        RandomNameData randomNameData = JsonUtility.FromJson<RandomNameData>(randomNameInfoRequest.downloadHandler.text);
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
        form.AddField("password", password);
        form.AddField("password_confirmation", password);

        UnityWebRequest request = UnityWebRequest.Post(registerUrl, form);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"{request.error}");
            yield break;
        }

        RegisterData registerData = JsonUtility.FromJson<RegisterData>(request.downloadHandler.text);
        string token = registerData.data.token;

        Debug.Log("Registration sucessful, token is "+token);

        //TO DO: check for errors even on a sucessful answer

        GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token); //we request the profile to confirm the server got our account created properly. This will invoke later EVENT_LOGIN_COMPLETED
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
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
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

        //TODO: check for errors even on sucessful result

        GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token);
    }     

    IEnumerator GetProfile(string token)
    {
        Debug.Log("Getting profile with token " +token);

        string profileUrl = $"{baseUrl}{urlProfile}";

        //UnityWebRequest profileInfoRequest = UnityWebRequest.Get($"{profileUrl}?Authorization={Uri.EscapeDataString(token)}");
        UnityWebRequest request = UnityWebRequest.Get(profileUrl); // TO DO: this should be asking for authorization on the header
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error getting profile with token "+token);
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

        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.Invoke(profileData);//TODO: these 2 events here don't look good
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke(name, fief);
                     
    }   

    IEnumerator GetLogout(string token)
    {
        string loginUrl = $"{baseUrl}{urlLogout}";
        WWWForm form = new WWWForm();

        UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
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

    IEnumerator GetExpeditionStatus()
    {
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("[RequestExpeditionStattus] with token " + token);

        string fullUrl = $"{baseUrl}{urlExpeditionStatus}";
        WWWForm form = new WWWForm();

        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
       // UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
        request.SetRequestHeader("Accept", "*/*");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        Debug.Log(request.GetRequestHeader("Authorization"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("[Error getting expedition status] " + request.error);
           
            yield break;
        }

        ExpeditionStatusData data = JsonUtility.FromJson<ExpeditionStatusData>(request.downloadHandler.text);

        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(data.GetHasExpedition());

        Debug.Log("answer from expedition status " + request.downloadHandler.text);
             
    }  

    IEnumerator GetCharacterList()
    {
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("[GetCharacterList] with token " + token);

        string fullUrl = $"{baseUrl}{urlCharactersList}";
        WWWForm form = new WWWForm();

        UnityWebRequest request = UnityWebRequest.Get($"{fullUrl}?Authorization={Uri.EscapeDataString(token)}");
        // UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
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

    public IEnumerator RequestNewExpedition(string characterType)
    {
        string fullURL = $"{baseUrl}{urlExpeditionRequest}";

        string token = PlayerPrefs.GetString("session_token");

        WWWForm form = new WWWForm();
        form.AddField("class", characterType);
      
        UnityWebRequest request = UnityWebRequest.Post(fullURL, form);
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Request new expedition error: "+$"{request.error}");
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

    private IEnumerator CancelOngoingExpedition()
    {
        string fullURL = $"{baseUrl}{urlExpeditionCancel}";
        string token = PlayerPrefs.GetString("session_token");
        
        UnityWebRequest request = UnityWebRequest.Post(fullURL, "");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("[Error canceling expedition]");
            yield break;
        }
        
        GameManager.Instance.EVENT_EXPEDITION_STATUS_UPDATE.Invoke(false);
        Debug.Log("answer from cancel expedition " + request.downloadHandler.text);
    }
}