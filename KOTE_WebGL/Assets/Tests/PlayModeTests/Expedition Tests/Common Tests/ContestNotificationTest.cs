using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ContestNotificationTest : MonoBehaviour
{
    ContestNotifications contestManager;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameObject Prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/ContestNotifications.prefab");
        GameObject Obj = GameObject.Instantiate(Prefab);
        contestManager = Obj.GetComponent<ContestNotifications>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(contestManager);
        yield return null;
    }

    [UnityTest]
    public IEnumerator WarningOpenPanel() 
    {
        bool eventCalled = false;
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.AddListener(
            (string s, Action a, Action b, string[] c) => { eventCalled = true; });
        contestManager.GiveWarning(60);
        yield return null;
        Assert.IsTrue(eventCalled);
    }

    [UnityTest]
    public IEnumerator EndContestOpenPanel()
    {
        bool eventCalled = false;
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.AddListener(
            (string s, Action a, Action b, string[] c) => { eventCalled = true; });
        contestManager.GiveContestEnded();
        yield return null;
        Assert.IsTrue(eventCalled);
    }
}
