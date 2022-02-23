using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class WebRequester : MonoBehaviour
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

    private readonly string baseUrl = "https://gateway.kote.robotseamonster.com";
    private readonly string urlRandomName = "/auth/v1/generate/username";
    private readonly string urlRegister = "/auth/v1/register";

    private void Start()
    {
        PlayerPrefs.SetString("session_token", "");
        PlayerPrefs.Save();
    }

    IEnumerator GetRandomName(TMP_Text nameText)
    {
        nameText.text = "Loading...";

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
        nameText.text = randomNameData.data.username;
    }

    public void RequestNewName(TMP_Text nameText)
    {
        StartCoroutine(GetRandomName(nameText));
    }

    IEnumerator GetRegister(string nameText, string email, string password)
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
        PlayerPrefs.SetString("session_token", registerData.data.token);
        PlayerPrefs.Save();
    }

    public void RequestRegister(string namesText, string email, string password)
    {
        StartCoroutine(GetRegister(namesText, email, password));
    }
}