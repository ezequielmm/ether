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
        Debug.Log($"[CombatQueue] [{queue.Count}] Action Enqueued... {data.ToString()}");

        //if (queue.Count == 0) // No delay on first hit in total
        //{
        //    data.delay = 0;
        //}
        foreach (CombatTurnData turn in queue) // No delay on multiple hits from the same party
        {
            if (turn.origin == data.origin)
            {
                data.delay = 0;
            }
        }

        queue.Enqueue(data);
    }

    private void OnTurnUnblock() 
    {
        Debug.Log($"[CombatQueue] Action Completed!");
        queue.Dequeue();
        awaitToContinue = false;
        if (queue.Count == 0) 
        {
            GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.Invoke();
        }
    }

    private void ProcessTurn(CombatTurnData data) 
    {
        Debug.Log($"[CombatQueue] [{queue.Count}] New Action {data.ToString()}  ******************");
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
