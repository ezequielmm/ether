using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class HandManagerTests
{
    private HandManager _handManager;
    [UnitySetUp]
    public void Setup()
    {
        GameObject handPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Hand.prefab");
        GameObject handPileManager = GameObject.Instantiate(handPrefab);
        _handManager = handPileManager.GetComponent<HandManager>();
    }

    [UnityTest]
    public IEnumerator DoesStartFireGenericDataEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) => { eventFired = true;});
        GameObject go = new GameObject();
        HandManager handManager = go.AddComponent<HandManager>();
        go.SetActive(true);
        handManager.enabled = true;
        yield return null;
        Assert.True(eventFired);
    }

    [Test]
    public void DoesDrawingACardFireSFXEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data) => { eventFired = true;});
        GameManager.Instance.EVENT_CARD_DRAW.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesDrawingACardFireCorrectSFX()
    {
        string sfxType = "";
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data) => { sfxType = data;});
        GameManager.Instance.EVENT_CARD_DRAW.Invoke();
        Assert.AreEqual("Card Draw", sfxType);
    }
}
