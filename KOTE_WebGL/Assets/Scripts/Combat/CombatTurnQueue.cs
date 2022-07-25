using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatTurnQueue : MonoBehaviour
{
    Queue<CombatTurnData> queue;
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
        queue.Enqueue(data);
    }

    private void OnTurnUnblock() 
    {
        queue.Dequeue();
        awaitToContinue = false;
        if (queue.Count == 0) 
        {
            GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.Invoke();
        }
    }

    private void ProcessTurn(CombatTurnData data) 
    {
        if (data.target == "player")
        {
            GameManager.Instance.EVENT_PLAYER_ATTACKED.Invoke(data);
            awaitToContinue = true;
        }
        else // Enemy
        {
            GameManager.Instance.EVENT_ENEMY_ATTACKED.Invoke(data);
            awaitToContinue = true;
        }
    }

    private void Update()
    {
        if (queue.Count > 0 && !awaitToContinue) 
        {
            ProcessTurn(queue.Peek());
        }
    }
}
