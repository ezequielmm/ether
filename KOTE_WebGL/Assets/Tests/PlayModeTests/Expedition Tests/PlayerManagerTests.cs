using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerManagerTests
{
    private PlayerManager playerManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject Prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/PlayerBaseNew.prefab");
        GameObject obj = GameObject.Instantiate(Prefab);
        playerManager = obj.GetComponent<PlayerManager>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameObject.Destroy(playerManager.gameObject);
        yield return null;
    }

    [Test]
    public void PlayerNameUpdated()
    {
        string name = "name";
        playerManager.SetNameAndFief(name, 22);
        Assert.AreEqual(name, playerManager.nameTextField.text);
    }
}