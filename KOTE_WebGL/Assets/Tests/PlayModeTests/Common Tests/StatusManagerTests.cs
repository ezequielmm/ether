using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class StatusManagerTests : MonoBehaviour
{
    private StatusManager _statusManager;
    private GameObject player;
    private GameObject nftManager;
    private GameObject spriteManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject playerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/PlayerBaseNew.prefab");
        player = Instantiate(playerPrefab);
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        playerManager.PlayerData = new PlayerData
        {
            cards = new List<Card>(),
            characterClass = "knight",
            defense = 1,
            energy = 2,
            energyMax = 2,
            gold = 0,
            hpMax = 1,
            hpCurrent = 1,
            id = "UnityTestsAreAnnoyingToGetWorkingSometimes"
        };
        _statusManager = player.GetComponentInChildren<StatusManager>();
        player.SetActive(true);
        
        GameObject nftManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/NftSpriteManager.prefab");
        nftManager = Instantiate(nftManagerPrefab);
        
        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(player);
        Destroy(nftManager);
        Destroy(spriteManager);
        GameManager.Instance.DestroyInstance();
        yield return null;
    }

    [UnityTest]
    public IEnumerator StatusManagerHasParentManagerAndSerializedFields()
    {
        GameObject go = new GameObject();
        go.AddComponent<StatusManager>();
        go.SetActive(true);
        yield return null;
        LogAssert.Expect(LogType.Error, "[StatusManager] Missing Icon Prefab.");
        LogAssert.Expect(LogType.Error, "[StatusManager] Missing Icon Container.");
        LogAssert.Expect(LogType.Error, "[StatusManager] Manager does not belong either an enemy or a player.");
        LogAssert.Expect(LogType.Exception,
            "NullReferenceException: Object reference not set to an instance of an object");
    }

    [UnityTest]
    public IEnumerator OnSetStatusCreatesStatus()
    {
        StatusData statusData = new StatusData
        {
            id = "UnityTestsAreAnnoyingToGetWorkingSometimes",
            statuses = new List<StatusData.Status>
                { new StatusData.Status { counter = 1, description = "test", name = "resolve" } },
            targetEntity = "player"
        };
        GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.Invoke(statusData);
        yield return null;
        // all this insanity is just to count the number of status icons created
        Transform[] statusIcons = _statusManager.gameObject.GetComponentsInChildren<Transform>();
        int count = 0;
        foreach (Transform transform in statusIcons)
        {
            if (transform.gameObject.name.Contains("Status Icon")) count++;
        }

        Assert.AreEqual(1, count);
    }

    [UnityTest]
    public IEnumerator AreStatusAskedForOnTurnChange()
    {
        bool eventFired = false;
        WS_DATA_REQUEST_TYPES resultType = WS_DATA_REQUEST_TYPES.Enemies;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((requestType) =>
        {
            eventFired = true;
            resultType = requestType;
        });
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke("");
        yield return null;
        Assert.True(eventFired);
        Assert.AreEqual(WS_DATA_REQUEST_TYPES.Statuses, resultType);
    }

    [UnityTest]
    public IEnumerator DoesUpdateStatusCreateStatusIcon()
    {
        List<StatusData.Status> statuses = new List<StatusData.Status>
            { new StatusData.Status { counter = 1, description = "test", name = "resolve" } };
        ;
        _statusManager.UpdateStatus(statuses);
        yield return null;
        // all this insanity is just to count the number of status icons created
        Transform[] statusIcons = _statusManager.gameObject.GetComponentsInChildren<Transform>();
        int count = 0;
        foreach (Transform transform in statusIcons)
        {
            if (transform.gameObject.name.Contains("Status Icon")) count++;
        }

        Assert.AreEqual(1, count);
    }
}