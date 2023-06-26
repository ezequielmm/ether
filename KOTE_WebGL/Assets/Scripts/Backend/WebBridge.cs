using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine.Events;

public class WebBridge : MonoBehaviour
{
    public static UnityEvent<string> OnWebMessageRecieved = new UnityEvent<string>();
    
    [DllImport("__Internal")]
    private static extern void GetUnityMessage(string eventName, string data);

    private static WebBridge instance = null;
    private void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public static void SendUnityMessage(string eventName, string data) {
        Debug.Log($"eventName {eventName} data {data}");
        GetUnityMessage(eventName, data);
    }


    public void OnWebMessage(string data)
    {
        OnWebMessageRecieved.Invoke(data);
    }
}