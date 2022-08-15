using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTurnQueue : MonoBehaviour
{
    Queue<CombatTurnData> queue;
    [SerializeField]
    bool awaitToContinue;

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
    }

    private void ClearCombatQueue() 
    {
        awaitToContinue = false;
        queue.Clear();
    }

    private void OnTurnUnblock(System.Guid attackId) 
    {
        if (queue.Count == 0) 
        {
            Debug.LogWarning($"[CombatQueue] Unblock called when queue was empty!");
            AwaitToContinue = false;
            return;
        }
        if (queue.Peek().attackId != attackId)
        {
            Debug.LogWarning($"[CombatQueue] Unblock called for {attackId} when {queue.Peek().attackId} was in queue!");
            //return;
        };
        Debug.Log($"[CombatQueue] Action Completed!");
        var last = queue.Peek();
        queue.Dequeue();
        AwaitToContinue = false;
        if (queue.Count == 0) 
        {
            GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.Invoke();
            Debug.Log($"[CombatQueue] End of Combat Queue!");
            queue.Clear();
        }
        else if (queue.Peek().originId != last.originId) // On Origin Change
        {
            GameManager.Instance.EVENT_COMBAT_ORIGIN_CHANGE.Invoke();
            Debug.Log($"[CombatQueue] New Origin for Events!");
        }
    }

    private void ProcessTurn(CombatTurnData data) 
    {
        Debug.Log($"[CombatQueue] [{queue.Count}] Action Being Run --==| {data.ToString()} |==--");
        GameManager.Instance.EVENT_ATTACK_REQUEST.Invoke(data);
        AwaitToContinue = true;
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
    }
}
