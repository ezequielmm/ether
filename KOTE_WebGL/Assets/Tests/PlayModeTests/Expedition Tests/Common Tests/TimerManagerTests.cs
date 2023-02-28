using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TimerManagerTests
{
    private TimerManager _timerManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject TimerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/Timer.prefab");
        GameObject Timer = GameObject.Instantiate(TimerPrefab);
        _timerManager = Timer.GetComponent<TimerManager>();

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        _timerManager.Reset();
        GameObject.Destroy(_timerManager.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesTimeCarryOverOnDestroy()
    {
        _timerManager.SetTimerStartTime(DateTime.UtcNow - TimeSpan.FromSeconds(500));
        GameObject.Destroy(_timerManager.gameObject);

        yield return null;
        GameObject TimerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/Timer.prefab");
        GameObject Timer = GameObject.Instantiate(TimerPrefab);
        _timerManager = Timer.GetComponent<TimerManager>();
        yield return null;

        Assert.GreaterOrEqual(_timerManager.TimePassed, 500);
    }

    [Test]
    public void DoesTimerStartTimeGetSet()
    {
        _timerManager.Reset();
        _timerManager.SetTimerStartTime(DateTime.UtcNow - TimeSpan.FromSeconds(323));
        Assert.GreaterOrEqual(_timerManager.TimePassed, 323);
    }

    [UnityTest]
    public IEnumerator DoesResetSetTimerToZero()
    {
        _timerManager.SetTimerStartTime(DateTime.UtcNow - TimeSpan.FromSeconds(323));
        yield return null;
        _timerManager.Reset();
        Assert.GreaterOrEqual(_timerManager.TimePassed, 0);
    }
}
