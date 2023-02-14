using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using map;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ClientEnvironmentManagerTest
{
    [TearDown]
    public void TearDown()
    {
        ClientEnvironmentManager.Instance.DestroyInstance();
    }

    [Test]
    public void ModifyInUnity()
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        // Act
        env.InUnity = false;
        // Assert
        Assert.IsFalse(env.InUnity);
    }

    [Test]
    [TestCase("localhost")]
    [TestCase("192.168.0.234")]
    [TestCase("127.0.0.1")]
    [TestCase("")]
    [TestCase("unity")]
    [TestCase("https://client.dev.kote.robotseamonster.com/")]
    [TestCase("https://client.stage.kote.robotseamonster.com/")]
    [TestCase("https://client.alpha.kote.robotseamonster.com/")]
    [TestCase("https://alpha.knightsoftheether.com/")]
    public void ForceNotInUnity(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreNotEqual(result, ClientEnvironmentManager.Environments.Unity);
    }

    [Test]
    [TestCase("dev")]
    [TestCase("Development")]
    [TestCase("kote.dev.client")]
    [TestCase("https://client.dev.kote.robotseamonster.com/")]
    [TestCase("super dev bros")]
    public void EnvironmentSelected_Dev(string testUrl) 
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.Dev);
    }

    [Test]
    [TestCase("localhost")]
    [TestCase("192.168.0.234")]
    [TestCase("127.0.0.1")]
    [TestCase("")]
    [TestCase("No specific mention")]
    public void EnvironmentSelected_Default(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.Unknown);
    }

    [Test]
    [TestCase("localhost")]
    [TestCase("192.168.0.234")]
    [TestCase("127.0.0.1")]
    [TestCase("")]
    [TestCase("unity")]
    [TestCase("https://client.dev.kote.robotseamonster.com/")]
    [TestCase("https://client.stage.kote.robotseamonster.com/")]
    [TestCase("https://client.alpha.kote.robotseamonster.com/")]
    [TestCase("https://alpha.knightsoftheether.com/")]
    public void EnvironmentSelected_Unity(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = true;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.Unity);
    }

    [Test]
    [TestCase("stage")]
    [TestCase("client.stage")]
    [TestCase("https://client.stage.kote.robotseamonster.com/")]
    public void EnvironmentSelected_Stage(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.Stage);
    }

    [Test]
    [TestCase("alpha.robotseamonster")]
    [TestCase("https://client.alpha.kote.robotseamonster.com/")]
    public void EnvironmentSelected_AlphaTest(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.AlphaTest);
    }

    [Test]
    [TestCase("alpha")]
    [TestCase("kote.alpha")]
    [TestCase("https://alpha.knightsoftheether.com/")]
    public void EnvironmentSelected_Alpha(string testUrl)
    {
        // Arrange
        ClientEnvironmentManager env = ClientEnvironmentManager.Instance;
        env.InUnity = false;
        // Act
        var result = env.DetermineEnvironment(testUrl);
        // Assert
        Assert.AreEqual(result, ClientEnvironmentManager.Environments.Alpha);
    }

}
