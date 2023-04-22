using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WebRequesterManagerTests : MonoBehaviour
{
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        WebRequesterManager.Instance.DestroyInstance();
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestValuesOnStart()
    {
        yield return null;
        Assert.IsNotNull(WebRequesterManager.Instance);
        Assert.AreEqual(true, WebRequesterManager.Instance.gameObject.scene.name == "DontDestroyOnLoad");
    }
}