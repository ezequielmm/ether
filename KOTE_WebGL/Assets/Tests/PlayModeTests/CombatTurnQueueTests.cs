using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CombatTurnQueueTests : MonoBehaviour
{
    private CombatTurnQueue _combatTurnQueue;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject go = new GameObject();
        _combatTurnQueue = go.AddComponent<CombatTurnQueue>();
        go.SetActive(true);
        yield return null;
    }

    [Test]
    public void DoesQueueAttackThrowWarningIfNoTurnDataProvided()
    {
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(null);
        LogAssert.Expect(LogType.Warning,"[CombatQueue] [0] Can not enqueue an empty Combat Action");
    }
    
    [Test]
    public void DoesQueueAttackThrowWarningIfNoOriginIdProvided()
    {
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(
            new CombatTurnData
            {
                attackId = new Guid(),
                delay = 0,
                originId = "",
                originType = "",
                targets = new List<CombatTurnData.Target>()
            });
        LogAssert.Expect(LogType.Warning,"[CombatQueue] [0] Can not enqueue a Combat Action missining an origin");
        
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(
            new CombatTurnData
            {
                attackId = new Guid(),
                delay = 0,
                originId = null,
                originType = "",
                targets = new List<CombatTurnData.Target>()
            });
        LogAssert.Expect(LogType.Warning,"[CombatQueue] [0] Can not enqueue a Combat Action missining an origin");
    }

    [Test]
    public void DoesQueueAttackThrowWarningIfNoTargetsProvided()
    {
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(
            new CombatTurnData
            {
                attackId = new Guid(),
                delay = 0,
                originId = "test",
                originType = "",
                targets = new List<CombatTurnData.Target>()
            });
        LogAssert.Expect(LogType.Warning,"[CombatQueue] [0] Can not enqueue a Combat Action missining a target");
    }

    [Test]
    public void DoesQueueAttackThrowWarningIfBadTargetsProvided()
    {
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(
            new CombatTurnData
            {
                attackId = new Guid(),
                delay = 0,
                originId = "test",
                originType = "",
                targets = new List<CombatTurnData.Target>{new CombatTurnData.Target{targetId = ""}}
            });
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Has a bad target... Removing it...");
        LogAssert.Expect(LogType.Warning,"[CombatQueue] [0] Can not enqueue a Combat Action missining a target");
    }

    [Test]
    public void DoesQueueEnqueueActionIfGoodDataProvided()
    {
        CombatTurnData data = new CombatTurnData
        {
            attackId = new Guid(),
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);
        LogAssert.Expect(LogType.Log, $"[CombatQueue] [0] Action Enqueued... {data.ToString()}");
    }
    
}