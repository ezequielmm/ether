using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class WebBridge : MonoBehaviour
{
    
    [DllImport("__Internal")]
    private static extern void GetUnityMessage(string eventName, string data);

    public static void SendUnityMessage(string eventName, string data) {
        Debug.Log($"eventName {eventName} data {data}");
        GetUnityMessage(eventName, data);
    }
}