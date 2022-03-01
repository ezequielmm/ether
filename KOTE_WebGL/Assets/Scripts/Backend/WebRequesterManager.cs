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

    private readonly string baseUrl = "https://gateway.kote.robotseamonster.com";
    private readonly string urlRandomName = "/auth/v1/generate/username";
    private readonly string urlRegister = "/auth/v1/register";
    private readonly string urlLogin = "/auth/v1/login";

    private void Start()
    {
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();

        GameManager.Instance.EVENT_REQUEST_NAME.AddListener(OnRandomNameEvent);
        GameManager.Instance.EVENT_REGISTER.AddListener(OnRegisterEvent);
        GameManager.Instance.EVENT_LOGIN.AddListener(RequestLogin);

        OnRandomNameEvent();
    }

    public IEnumerator GetRandomName()
    {
        GameManager.Instance.EVENT_NEW_RANDOM_NAME.Invoke("Loading...");

        string randomNameUrl = $"{baseUrl}{urlRandomName}";

        UnityWebRequest randomNameInfoRequest = UnityWebRequest.Get(randomNameUrl);

        yield return randomNameInfoRequest.SendWebRequest();

        if (randomNameInfoRequest.result == UnityWebRequest.Result.ConnectionError ||
            randomNameInfoRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"{randomNameInfoRequest.error}");
            yield break;
        }

        RandomNameData randomNameData =
            JsonUtility.FromJson<RandomNameData>(randomNameInfoRequest.downloadHandler.text);
        string newName = randomNameData.data.username;

        GameManager.Instance.EVENT_NEW_RANDOM_NAME.Invoke(string.IsNullOrEmpty(newName) ? "" : newName);
    }

    public void OnRandomNameEvent()
    {
        StartCoroutine(GetRandomName());
    }

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

        GameManager.Instance.EVENT_REGISTER_COMPLETED.Invoke(string.IsNullOrEmpty(token) ? "" : token);
    }

    public void OnRegisterEvent(string name, string email, string password)
    {
        StartCoroutine(GetRegister(name, email, password));
    }

    IEnumerator GetLogin(string email, string password, bool rememberMe)
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
            GameManager.Instance.EVENT_LOGIN_COMPLETED.Invoke("", false);
            yield break;
        }

        LoginData registerData = JsonUtility.FromJson<LoginData>(request.downloadHandler.text);
        string token = registerData.data.token;

        GameManager.Instance.EVENT_LOGIN_COMPLETED.Invoke(token, true);
    }

    public void RequestLogin(string email, string password, bool rememberMe)
    {
        StartCoroutine(GetLogin(email, password, rememberMe));
    }
}