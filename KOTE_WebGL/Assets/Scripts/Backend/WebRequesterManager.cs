using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class WebRequesterManager : MonoBehaviour
{
    [Serializable]
    public class RandomNameData
    {
        public Data data;

        [Serializable]
        public class Data
        {
            public string username;
        }
    }

    [Serializable]
    public class RegisterData
    {
        public Data data;

        [Serializable]
        public class Data
        {
            public string token;
            public string name;
        }
    }

    [Serializable]
    public class LoginData
    {
        public Data data;

        [Serializable]
        public class Data
        {
            public string token;
        }
    }

    [Serializable]
    public class ProfileData
    {
        public Data data;

        [Serializable]
        public class Data
        {
            public string id;
            public string name;
            public string email;
            public List<string> wallets;
            public int coins;
            public int fief;
            public int experience;
            public int level;
            public int act;
            public ActMap act_map;

            [Serializable]
            public class ActMap
            {
                public string id;
                public string current_node;
            }
        }
    }

    [Serializable]
    public class LogoutData
    {
        public Data data;

        [Serializable]
        public class Data
        {
            public string message;
        }
    }

    private readonly string baseUrl = "https://gateway.kote.robotseamonster.com";
    private readonly string urlRandomName = "/auth/v1/generate/username";
    private readonly string urlRegister = "/auth/v1/register";
    private readonly string urlLogin = "/auth/v1/login";
    private readonly string urlProfile = "/gsrv/v1/profile";
    private readonly string urlLogout = "/auth/v1/logout";

    private void Awake()
    {
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();

        GameManager.Instance.EVENT_REQUEST_NAME.AddListener(OnRandomNameEvent);
        GameManager.Instance.EVENT_REQUEST_REGISTER.AddListener(OnRegisterEvent);
        GameManager.Instance.EVENT_REQUEST_LOGIN.AddListener(RequestLogin);
        GameManager.Instance.EVENT_REQUEST_PROFILE.AddListener(RequestProfile);
        GameManager.Instance.EVENT_REQUEST_LOGOUT.AddListener(RequestLogout);
    }

    private void Start()
    {
        GameManager.Instance.webRequester = this;
        DontDestroyOnLoad(this);
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

    public void OnRandomNameEvent(string previousName)
    {
        StartCoroutine(GetRandomName(previousName));
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

        //TO DO: check for errors even on a sucessful answer

        GameManager.Instance.EVENT_REQUEST_PROFILE.Invoke(token); //we request the profile to confirm the server got our account created properly. This will invoke later EVENT_LOGIN_COMPLETED
    }

    public void OnRegisterEvent(string name, string email, string password)
    {
        StartCoroutine(GetRegister(name, email, password));
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

    public void RequestLogin(string email, string password)
    {
        StartCoroutine(GetLogin(email, password));
    }

    IEnumerator GetProfile(string token)
    {
        Debug.Log("Getting profile");

        string profileUrl = $"{baseUrl}{urlProfile}";

        UnityWebRequest profileInfoRequest = UnityWebRequest.Get($"{profileUrl}?Authorization={Uri.EscapeDataString(token)}");

        yield return profileInfoRequest.SendWebRequest();

        if (profileInfoRequest.result == UnityWebRequest.Result.ConnectionError ||
            profileInfoRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"{profileInfoRequest.error}");
            yield break;
        }

        //TODO: check for errors even when the result is sucessful

        ProfileData profileData = JsonUtility.FromJson<ProfileData>(profileInfoRequest.downloadHandler.text);
        string name = profileData.data.name;
        int fief = profileData.data.fief;

        PlayerPrefs.SetString("session_token", token);
        PlayerPrefs.Save();

        GameManager.Instance.EVENT_REQUEST_PROFILE_SUCCESSFUL.Invoke(profileData);
        GameManager.Instance.EVENT_REQUEST_LOGIN_SUCESSFUL.Invoke(name, fief);
    }

    public void RequestProfile(string token)
    {
        StartCoroutine(GetProfile(token));
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

    public void RequestLogout(string token)
    {
        StartCoroutine(GetLogout(token));
    }
}