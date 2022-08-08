using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTurnQueue : MonoBehaviour
{
    Queue<CombatTurnData> queue;
    [SerializeField]
    bool awaitToContinue;

    void Start()
    {
        awaitToContinue = false;
        queue = new Queue<CombatTurnData>();
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.AddListener(QueueAttack);
        GameManager.Instance.EVENT_COMBAT_TURN_END.AddListener(OnTurnUnblock);
    }

    private void QueueAttack(CombatTurnData data) 
    {
        if (data.attackId == System.Guid.Empty)
        {
            data.attackId = System.Guid.NewGuid();
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

    private void OnTurnUnblock(System.Guid attackId) 
    {
        if (queue.Count == 0) return;
        if (queue.Peek().attackId != attackId) return;
        Debug.Log($"[CombatQueue] Action Completed!");
        var last = queue.Peek();
        queue.Dequeue();
        awaitToContinue = false;
        if (queue.Count == 0) 
        {
            GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.Invoke();
        } 
        else if (queue.Peek().originId != last.originId) // On Origin Change
        {
            GameManager.Instance.EVENT_COMBAT_ORIGIN_CHANGE.Invoke();
        }
    }

    private void ProcessTurn(CombatTurnData data) 
    {
        Debug.Log($"[CombatQueue] [{queue.Count}] Action Being Run --==| {data.ToString()} |==--");
        GameManager.Instance.EVENT_ATTACK_REQUEST.Invoke(data);
        awaitToContinue = true;
    }

    private void Update()
    {
        if (queue.Count > 0 && !awaitToContinue) 
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
