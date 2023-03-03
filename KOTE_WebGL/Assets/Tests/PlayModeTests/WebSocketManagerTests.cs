using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WebSocketManagerTests
{
    private WebSocketManager webSocket;
    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/Expedition");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        //webSocket = GameObject.Find("WebSocketManager").GetComponent<WebSocketManager>();
    }

    [UnityTest]
    public IEnumerator WebSocketGameObjectExists()
    {
        GameObject socketObject = GameObject.Find("WebSocketManager");
        yield return null;
        Assert.IsNotNull(socketObject);
    }

    [UnityTest]
    public IEnumerator WebSocketScriptExists()
    {
        WebSocketManager socketScript = GameObject.Find("WebSocketManager").GetComponent<WebSocketManager>();
        yield return null;
        Assert.IsNotNull(socketScript);
    }
}
