using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class WebRequesterManagerTests 
{
    private WebRequesterManager requestManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/MainMenu");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        requestManager = GameObject.Find("WebRequesterManager").GetComponent<WebRequesterManager>();
    }

    [UnityTest]
    public IEnumerator DoesWebRequesterManagerObjectExist()
    {
        GameObject requestManagerObject = GameObject.Find("WebRequesterManager");
        yield return null;
        Assert.IsNotNull(requestManagerObject);
    }

    [UnityTest]
    public IEnumerator DoesWebRequesterManagerScriptExist()
    {
        WebRequesterManager testManager =GameObject.Find("WebRequesterManager").GetComponent<WebRequesterManager>();
        yield return null;
        Assert.IsNotNull(testManager);
    }

    [UnityTest]
    public IEnumerator TestSavedValuesOnAwake()
    {
        yield return null;
        string apiUrl = PlayerPrefs.GetString("api_url");
        Assert.AreEqual("https://gateway.dev.kote.robotseamonster.com", apiUrl);
        string sessionToken = PlayerPrefs.GetString("session_token");
        Assert.AreEqual("", sessionToken);
    }

    [UnityTest]
    public IEnumerator TestValuesOnStart()
    {
        yield return null;
        Assert.IsNotNull(GameManager.Instance.webRequester);
        Assert.AreEqual(true, requestManager.gameObject.scene.name == "DontDestroyOnLoad");
    }
}
