using KOTE.UI.Armory;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SetUpFixture]
public class UnitTestTests : MonoBehaviour
{
    [Test]
    public void InUnitTestTrue()
    {
        Assert.IsTrue(UnitTestDetector.IsInUnitTest);
    }

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        UnitTestDetector.IsInUnitTest = true;
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
    }
}
