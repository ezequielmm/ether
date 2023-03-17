using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class UserDataManagerTests : MonoBehaviour
{
    private UserDataManager userData;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        userData = UserDataManager.Instance;
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        userData.DestroyInstance();
        yield return null;
    }

    [Test]
    public void ClientIdNotNull()
    {
        Assert.NotNull(userData.ClientId);
    }
    
    [Test]
    public void ClientIdNotRandom()
    {
        Assert.AreEqual(userData.ClientId, userData.ClientId);
    }
    
    [Test]
    public void ClientIdIsPersistant()
    {
        Guid id = Guid.NewGuid();
        PlayerPrefs.SetString("client_id", id.ToString());
        Assert.AreEqual(id.ToString(), userData.ClientId);
    }
    
    [Test]
    public void SessionTokenSet()
    {
        string token = "token";
        userData.SetSessionToken(token);
        Assert.AreEqual(token, PlayerPrefs.GetString("session_token"));
    }
    
    [Test]
    public void SessionTokenGet()
    {
        string token = "token";
        userData.SetSessionToken(token);
        Assert.AreEqual(PlayerPrefs.GetString("session_token"), userData.GetSessionToken());
    }
}
