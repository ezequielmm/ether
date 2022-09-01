using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndTurnManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool listenForEmptyQueue = false;
    float timeLimit;
    bool isPlayersTurn = true;
    float originalPos => desiredLocation.position.x;
    RectTransform rectTransform;

    public Transform desiredLocation;

    void Start()
    {
        this.transform.position = desiredLocation.position;
        rectTransform = transform as RectTransform;
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
            transform.DOMoveX(originalPos, 0.5f).SetDelay(4);
            listenForEmptyQueue = false;
            isPlayersTurn = true;
        }
        else if(who == "enemy")
        {
            transform.DOMoveX(originalPos + Screen.width * 0.15f, 0.5f);
            listenForEmptyQueue = true;
            timeLimit += 3; // This is only because sometimes the enemy does nothing
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(desiredLocation.position.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) * 0.5f),
            desiredLocation.position.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) * 0.5f), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.EndTurnButtonTooltips, TooltipController.Anchor.BottomRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
