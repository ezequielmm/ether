using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTurnQueue : MonoBehaviour
{
    Queue<CombatTurnData> queue;
    [SerializeField]
    //[ReadOnly]
    bool awaitToContinue;
    float skipAwaitCounter;

    [SerializeField] 
    //[ReadOnly]
    QueueState queueState;
        
    enum QueueState {
        idle = 0,
        awaitingRun,
        requst,
        response
    }

    bool AwaitToContinue { 
        get => awaitToContinue; 
        set => awaitToContinue = value;
    }

    void Start()
    {
        AwaitToContinue = false;
        queue = new Queue<CombatTurnData>();
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.AddListener(QueueAttack);
        GameManager.Instance.EVENT_COMBAT_TURN_END.AddListener(OnTurnUnblock);
        GameManager.Instance.EVENT_ATTACK_RESPONSE.AddListener(OnResponseCall);
        GameManager.Instance.EVENT_COMBAT_FORCE_CLEAR.AddListener(ClearCombatQueue);
        queueState = QueueState.idle;
    }

    private void QueueAttack(CombatTurnData data) 
    {
        if (data == null) 
        {
            Debug.LogWarning($"[CombatQueue] [{queue.Count}] Can not enqueue an empty Combat Action");
            return;
        }

        if (data.attackId == System.Guid.Empty)
        {
            data.attackId = System.Guid.NewGuid();
        }

        if (string.IsNullOrEmpty(data.originId)) 
        {
            Debug.LogWarning($"[CombatQueue] [{queue.Count}] Can not enqueue a Combat Action missining an origin");
            return;
        }
        for(int i = 0; i < data.targets.Count; i++)
        {
            if (string.IsNullOrEmpty(data.targets[i].targetId))
            {
                Debug.LogWarning($"[CombatQueue] [{queue.Count}] Has a bad target... Removing it...");
                data.targets.RemoveAt(i);
                i--;
            }
        }
        if (data.targets.Count == 0)
        {
            Debug.LogWarning($"[CombatQueue] [{queue.Count}] Can not enqueue a Combat Action missining a target");
            return;
        }

        Debug.Log($"[CombatQueue] [{queue.Count}] Action Enqueued... {data.ToString()}");

        //if (queue.Count == 0) // No delay on first hit in total
        //{
        //    data.delay = 0;
        //}
        foreach (CombatTurnData turn in queue) // No delay on multiple hits from the same party
        {
            if (turn.originId == data.originId)
            {
                data.delay = 0;
            }
        }

        queue.Enqueue(data);
        if (queueState == QueueState.idle) 
        {
            queueState = QueueState.awaitingRun;
        }
    }

    private void ClearCombatQueue() 
    {
        awaitToContinue = false;
        Debug.Log($"[CombatQueue] Cleared with [{queue.Count}] animations left.");
        queue.Clear();
        queueState = QueueState.idle;
    }

    private void OnTurnUnblock(System.Guid attackId) 
    {
        if (queue.Count == 0) 
        {
            Debug.LogWarning($"[CombatQueue] Unblock called when queue was empty!");
            queueState = QueueState.idle;
            AwaitToContinue = false;
            return;
        }
        if (queue.Peek().attackId != attackId)
        {
            Debug.LogWarning($"[CombatQueue] Unblock called for {attackId} when {queue.Peek().attackId} was in queue!");
            //return;
        };
        if (queueState == QueueState.requst) 
        {
            Debug.Log($"[CombatQueue] Combat Response was not run for {queue.Peek()}");
        }
        Debug.Log($"[CombatQueue] Action Completed!");
        var last = queue.Peek();
        queue.Dequeue();
        AwaitToContinue = false;
        if (queue.Count == 0) 
        {
            Debug.Log($"[CombatQueue] End of Combat Queue!");
            queue.Clear();
            queueState = QueueState.idle;

            GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.Invoke();
        }
        else if (queue.Peek().originId != last.originId) // On Origin Change
        {
            Debug.Log($"[CombatQueue] New Origin for Events!");
            queueState = QueueState.awaitingRun;

            GameManager.Instance.EVENT_COMBAT_ORIGIN_CHANGE.Invoke();
        }
    }

    private void ProcessTurn(CombatTurnData data) 
    {
        Debug.Log($"[CombatQueue] [{queue.Count}] {data.action?.name} Action Being Run --==| {data.ToString()} |==--");
        AwaitToContinue = true;
        queueState = QueueState.requst;
        skipAwaitCounter = 3;

        GameManager.Instance.EVENT_ATTACK_REQUEST.Invoke(data);
    }

    private void OnResponseCall(CombatTurnData data) 
    {
        skipAwaitCounter += 3;
        queueState = QueueState.response;
    }

    private void Update()
    {
        if (queue.Count > 0 && !AwaitToContinue)
        {
            if (queue.Peek().delay > 0)
            {
                queue.Peek().delay -= Time.deltaTime;
            }
            else
            {
                ProcessTurn(queue.Peek());
            }
        }
        else if(AwaitToContinue)
        {
            skipAwaitCounter = Mathf.Max(skipAwaitCounter - Time.deltaTime, 0);
            if (skipAwaitCounter == 0) 
            {
                if (queue.Count > 0)
                {
                    var turnData = queue.Peek();
                    OnTurnUnblock(turnData.attackId);
                    Debug.LogWarning($"[CombatQueue] Combat Queue was not properly closed before the animation timed out. {turnData.ToString()}");
                }
                else 
                {
                    Debug.LogError($"[CombatQueue] Combat Queue timed out but the queue was empty.");
                }
            }
            if (queue.Count == 0) 
            {
                ClearCombatQueue();
                Debug.LogWarning($"[CombatQueue] Combat Queue was not properly closed before clearing part of queue.");
            }
        }
    }
}
