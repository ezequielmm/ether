using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class WebBridge : MonoBehaviour
{
    
    [DllImport("__Internal")]
    private static extern void GetUnityMessage(string body);

    public static void SendUnityMessage(string eventName, string data) {

        var message = new Payload { data = data, eventName = eventName};
        GetUnityMessage(JsonConvert.SerializeObject(message));
    }
}


[System.Serializable]
public class Payload{
    public string eventName;
    public string data;
}
