using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ContestManagerTests : MonoBehaviour
{
    ContestManager contestManager;

    [UnitySetUp]
    public IEnumerator SetUp() 
    {
        contestManager = ContestManager.Instance;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown() 
    {
        contestManager.DestroyInstance();
        contestManager = null;
        yield return null;
    }

    [Test]
    public void EndTimeSet() 
    {
        DateTime endTime = DateTime.Now.AddDays(1);
        contestManager.SetNewContestTime(endTime);

        Assert.AreEqual(endTime, contestManager.ContestEndTimeUtc);
    }

    [Test]
    public void TimeUntilEnd()
    {
        double dayInSeconds = 864000;

        DateTime endTime = DateTime.Now.AddDays(1);
        contestManager.SetNewContestTime(endTime);

        Assert.AreEqual(dayInSeconds, contestManager.TimeUntilEnd.TotalSeconds, 5);
    }
}
