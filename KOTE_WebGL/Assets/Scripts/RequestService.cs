using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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

    public IEnumerator GetRequestCoroutine(string url, Action<string> callback,  Action<string> error = null)
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

    public IEnumerator GetRequestCoroutine<T>(string url, Action<T> success, Action<string> error = null)
    {
        yield return GetRequestCoroutine(url, (str) =>
        {
            try
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(str);
                str = Encoding.UTF8.GetString(utf8Bytes);
                var data = JsonConvert.DeserializeObject<T>(str);
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