using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PracticeTests
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
    }
    
    // A Test behaves as an ordinary method
    [Test]
    public void PracticeStandardTest()
    {
        // Use the Assert class to test conditions
        string result = Utils.FindEntityId(new GameObject());
        Assert.AreEqual("unknown", result);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator PracticeSceneLoadTest()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        GameObject mainMenu = GameObject.Find("MainMenu");
        yield return null;
        Assert.IsNotNull(mainMenu);
    }
}
