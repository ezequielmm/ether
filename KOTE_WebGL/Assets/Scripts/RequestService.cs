using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RequestService : MonoBehaviour
{
    private static RequestService instance;
    public static RequestService Instance
    { get {
            if (instance == null) {
                instance = new GameObject("RequestService").AddComponent<RequestService>();
                DontDestroyOnLoad(Instance.gameObject);
            }

            return instance;
        }
    }

    public IEnumerator GetRequestCoroutine(string url, Action<string> callback,  Action<string> error)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.AddAuthToken();
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                error?.Invoke(webRequest.error);
            }
            else
            {
                callback?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetRequestCoroutine<T>(string url, Action<T> success, Action<string> error)
    {
        yield return GetRequestCoroutine(url, (str) =>
        {
            try
            {
                var data = JsonUtility.FromJson<T>(str);
                success?.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                error?.Invoke(e.Message);
            }
        }, error);
    }
    
    
}