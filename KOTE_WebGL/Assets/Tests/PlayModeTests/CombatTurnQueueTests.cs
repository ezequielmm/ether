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
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Can not enqueue an empty Combat Action");
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
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Can not enqueue a Combat Action missining an origin");

        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(
            new CombatTurnData
            {
                attackId = new Guid(),
                delay = 0,
                originId = null,
                originType = "",
                targets = new List<CombatTurnData.Target>()
            });
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Can not enqueue a Combat Action missining an origin");
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
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Can not enqueue a Combat Action missining a target");
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
                targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "" } }
            });
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Has a bad target... Removing it...");
        LogAssert.Expect(LogType.Warning, "[CombatQueue] [0] Can not enqueue a Combat Action missining a target");
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

    [Test]
    public void DoesTurnUnblockThrowWarningIfQueueIsEmpty()
    {
        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(new Guid());
        LogAssert.Expect(LogType.Warning, "[CombatQueue] Unblock called when queue was empty!");
    }

    [Test]
    public void DoesTurnUnblockThrowWarningIfWrongAttackCalled()
    {
        Guid goodId = Guid.NewGuid();
        Guid badId = new Guid();

        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(new Guid());
        LogAssert.Expect(LogType.Warning, $"[CombatQueue] Unblock called for {badId} when {goodId} was in queue!");
    }

    [Test]
    public void DoesTurnUnblockLogThatActionWasCompleted()
    {
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);
        LogAssert.Expect(LogType.Log, $"[CombatQueue] Action Completed!");
    }

    [Test]
    public void DoesTurnUnblockLogThatEndOfQueueWasReached()
    {
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);
        LogAssert.Expect(LogType.Log, $"[CombatQueue] End of Combat Queue!");
    }

    [Test]
    public void DoesTurnUnblockFireCombatQueueEmptyEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.AddListener(() => { eventFired = true; });
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesTurnUnblockLogNewOriginForEvents()
    {
        Guid goodId = Guid.NewGuid();
        Guid goodId2 = Guid.NewGuid();

        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        });
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(new CombatTurnData
        {
            attackId = goodId2,
            delay = 0,
            originId = "test2",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        });
        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);

        LogAssert.Expect(LogType.Log, "[CombatQueue] New Origin for Events!");
    }

    [Test]
    public void DoesTurnUnblockFireCombatOriginChangeEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_COMBAT_ORIGIN_CHANGE.AddListener(() => { eventFired = true; });
        Guid goodId = Guid.NewGuid();
        Guid goodId2 = Guid.NewGuid();

        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        });
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(new CombatTurnData
        {
            attackId = goodId2,
            delay = 0,
            originId = "test2",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        });

        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesTurnUnblockLogThatResponseWasNotRun()
    {
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        yield return null;
        GameManager.Instance.EVENT_COMBAT_TURN_END.Invoke(goodId);
        LogAssert.Expect(LogType.Log, $"[CombatQueue] Combat Response was not run for {data}");
    }

    [UnityTest]
    public IEnumerator DoesProcessTurnLogThatActionIsBeingRun()
    {
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        yield return null;
        LogAssert.Expect(LogType.Log, $"[CombatQueue] [{1}] Action Being Run --==| {data.ToString()} |==--");
    }

    [UnityTest]
    public IEnumerator DoesProcessTurnFireAttackRequest()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener((data) => { eventFired = true; });
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "test" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);

        yield return null;
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesUpdateThrowWarningThatQueueWasNotProperlyClosed()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_ATTACK_REQUEST.AddListener((data) => { eventFired = true; });
        Guid goodId = Guid.NewGuid();
        CombatTurnData data = new CombatTurnData
        {
            attackId = goodId,
            delay = 0,
            originId = "test",
            originType = "test",
            targets = new List<CombatTurnData.Target> { new CombatTurnData.Target { targetId = "" } }
        };
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(data);
        
        yield return new WaitForSeconds(0.2f);
        LogAssert.Expect(LogType.Warning,
            $"[CombatQueue] Combat Queue was not properly closed before the animation timed out. {data.ToString()}");
    }
}