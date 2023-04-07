using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ZRotateTests : MonoBehaviour
{
    ZAxisRotate rotateable;

    [SetUp]
    public void SetUp() 
    {
        GameObject obj = new GameObject();
        rotateable = obj.AddComponent<ZAxisRotate>();
    }

    [TearDown]
    public void TearDown() 
    {
        Destroy(rotateable.gameObject);
    }

    [UnityTest]
    public IEnumerator CounterclockwiseRotation() 
    {
        rotateable.Multiplier = 1.0f;
        rotateable.transform.rotation = Quaternion.identity;
        yield return null;
        Vector3 eulars = rotateable.transform.eulerAngles;
        Assert.IsTrue(0 < eulars.z);
    }

    [UnityTest]
    public IEnumerator ClockwiseRotation()
    {
        rotateable.Multiplier = -1.0f;
        rotateable.transform.rotation = Quaternion.identity;
        yield return null;
        Vector3 eulars = rotateable.transform.eulerAngles;
        Assert.IsTrue(0 > eulars.z);
    }

    [UnityTest]
    public IEnumerator NoRotation()
    {
        rotateable.Multiplier = 0f;
        rotateable.transform.rotation = Quaternion.identity;
        yield return null;
        Vector3 eulars = rotateable.transform.eulerAngles;
        Assert.AreEqual(0, eulars.z, 0.001f);
    }
}
