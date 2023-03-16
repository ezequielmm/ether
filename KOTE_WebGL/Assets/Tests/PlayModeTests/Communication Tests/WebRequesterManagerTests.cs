using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WebRequesterManagerTests : MonoBehaviour
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

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(requestManager.gameObject);
        yield return null;
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
        WebRequesterManager testManager = GameObject.Find("WebRequesterManager").GetComponent<WebRequesterManager>();
        yield return null;
        Assert.IsNotNull(testManager);
    }

    [UnityTest]
    public IEnumerator TestValuesOnStart()
    {
        yield return null;
        Assert.IsNotNull(WebRequesterManager.Instance);
        Assert.AreEqual(true, WebRequesterManager.Instance.gameObject.scene.name == "DontDestroyOnLoad");
    }
}