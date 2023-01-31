using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndTurnManager : MonoBehaviour
{
    [SerializeField]
    EndTurnButtonManager endTurnButtonManager;

    bool listenForEmptyQueue = false;
    float timeLimit;
    bool isPlayersTurn = true;

    void Start()
    {
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(onTurnChange);
        GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.AddListener(onQueueEmpty);
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.AddListener(onQueueActionEnqueue);
    }

    void onQueueEmpty() {
        if (listenForEmptyQueue) 
        {
            listenForEmptyQueue = false;
            GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
        }
    }

    private void Update()
    {
        if (timeLimit > 0 && listenForEmptyQueue) 
        {
            timeLimit -= Time.deltaTime;
            if (timeLimit < 0) 
            {
                Debug.LogWarning($"[EndTurnManager] Enemy Turn Empty Queue Timelimit (3s). No attack present?");
                GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
                GameManager.Instance.EVENT_CLEAR_COMBAT_QUEUE.Invoke();
                timeLimit = 0;
            }
        }
    }

    void onQueueActionEnqueue(CombatTurnData data) 
    {
        timeLimit = 0;
    }

    void onTurnChange(string who) 
    {
        if (who == "player")
        {
            endTurnButtonManager.Enable();
            listenForEmptyQueue = false;
            isPlayersTurn = true;
        }
        else if(who == "enemy")
        {
            endTurnButtonManager.Disable();
            listenForEmptyQueue = true;
            timeLimit += 3; // This is only because sometimes the enemy does nothing
            isPlayersTurn = false;
        }
    }

    public void EndTurn()
    {
        if (isPlayersTurn)
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
        }
    }

    
}
