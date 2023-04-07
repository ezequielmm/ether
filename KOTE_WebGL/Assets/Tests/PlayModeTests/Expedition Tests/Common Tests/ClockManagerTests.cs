using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ClockManagerTests
{
    private ClockManager clockManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject ClockPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/Clock.prefab");
        GameObject Timer = GameObject.Instantiate(ClockPrefab);
        clockManager = Timer.GetComponent<ClockManager>();

        clockManager.ShowMilliSeconds = false;
        clockManager.AlwaysShowMilliSeconds = false;
        clockManager.ShowSeconds = false;
        clockManager.AlwaysShowMinutes = false;
        clockManager.AlwaysShowHours = false;
        clockManager.AlwaysShowUnits = false;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameObject.Destroy(clockManager.gameObject);
        yield return null;
    }

    [Test]
    [TestCase(0, "0.000s")]
    [TestCase(1.523f, "1.523s")]
    [TestCase(4.523852f, "4.523s")]
    [TestCase(59, "59.000s")]
    [TestCase(60, "1:00")]
    [TestCase(1484, "24:44")]
    [TestCase(3600, "1:00:00")]
    [TestCase(3661, "1:01:01")]
    [TestCase(86400, "24:00:00")]
    public void ShowMillisAndSecondsString(float time, string expectedString)
    {
        clockManager.ShowMilliSeconds = true;
        clockManager.ShowSeconds = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(0, "0s")]
    [TestCase(1.523f, "1s")]
    [TestCase(4.523852f, "4s")]
    [TestCase(59, "59s")]
    [TestCase(60, "1:00")]
    [TestCase(1484, "24:44")]
    [TestCase(3600, "1:00:00")]
    [TestCase(3661, "1:01:01")]
    [TestCase(86400, "24:00:00")]
    public void RawClockBehavior(float time, string expectedString)
    {
        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(0, "0m")]
    [TestCase(59, "0m")]
    [TestCase(60, "1m")]
    [TestCase(1484, "24m")]
    [TestCase(3600, "1:00")]
    [TestCase(3661, "1:01")]
    [TestCase(86400, "24:00")]
    public void ForceMinutesNoSeconds(float time, string expectedString)
    {
        clockManager.AlwaysShowMinutes = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(0, "0h")]
    [TestCase(59, "0h")]
    [TestCase(60, "0h")]
    [TestCase(1484, "0h")]
    [TestCase(3600, "1h")]
    [TestCase(3661, "1h")]
    [TestCase(86400, "24h")]
    [TestCase(360000, "100h")]
    public void ForceHoursNoSeconds(float time, string expectedString)
    {
        clockManager.AlwaysShowHours = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(0, "0:00")]
    [TestCase(59, "0:00")]
    [TestCase(60, "0:01")]
    [TestCase(1484, "0:24")]
    [TestCase(3600, "1:00")]
    [TestCase(3661, "1:01")]
    [TestCase(86400, "24:00")]
    [TestCase(360000, "100:00")]
    public void ForceHoursAndMinutesNoSeconds(float time, string expectedString)
    {
        clockManager.AlwaysShowHours = true;
        clockManager.AlwaysShowMinutes = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(0, "0s")]
    [TestCase(60, "1:00s")]
    public void ForceUnitsSeconds(float time, string expectedString)
    {
        clockManager.AlwaysShowUnits = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(60, "1m")]
    [TestCase(3600, "1:00m")]
    public void ForceUnitsMinutes(float time, string expectedString)
    {
        clockManager.AlwaysShowUnits = true;
        clockManager.AlwaysShowMinutes = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }

    [Test]
    [TestCase(3600, "1h")]
    [TestCase(360000, "100h")]
    public void ForceUnitsHours(float time, string expectedString)
    {
        clockManager.AlwaysShowUnits = true;
        clockManager.AlwaysShowHours = true;

        clockManager.TotalSeconds = time;
        string clockTime = clockManager.ToString();
        Assert.AreEqual(expectedString, clockTime);
    }
}

