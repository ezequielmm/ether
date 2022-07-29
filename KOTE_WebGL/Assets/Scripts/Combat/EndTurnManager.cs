using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnManager : MonoBehaviour
{
    bool listenForEmptyQueue = false;
    float timeLimit;
    bool isPlayersTurn = true;
    float originalPos;
    RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(onTurnChange);
        GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.AddListener(onQueueEmpty);
        originalPos = transform.position.x;
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
            if (timeLimit <= 0) 
            {
                GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
            }
        }
    }

    void onTurnChange(string who) 
    {
        if (who == "player")
        {
            transform.DOMoveX(originalPos, 0.5f);
            listenForEmptyQueue = false;
            isPlayersTurn = true;
        }
        else if(who == "enemy")
        {
            transform.DOMoveX(originalPos + rt.rect.width + 5, 0.5f);
            listenForEmptyQueue = true;
            timeLimit += 10; // This is only because sometimes the enemy does nothing
            isPlayersTurn = false;
        }
    }

    public void EndTurn()
    {
        if (isPlayersTurn)
        {
            GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
        }
    }
}
