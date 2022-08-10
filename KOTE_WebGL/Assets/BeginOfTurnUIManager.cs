using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class BeginOfTurnUIManager : MonoBehaviour
{
    public TextMeshProUGUI playerLabel;
    public TextMeshProUGUI enemyLabel;

    bool firstPlay = true;
    string lastTurn;
    bool inAnimation = false;
    bool animationInterrupted = false;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(OnBeginOfTurn);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener(OnMapPanelToggle);
        DeactivateLabels();
    }

    
    void OnBeginOfTurn(string who)
    {
        Debug.Log("[OnBeginOfTurn]who: " + who);
        lastTurn = who;
        inAnimation = true;
        animationInterrupted = false;
        switch (who)
        {
            case "player":
                playerLabel.gameObject.SetActive(true);
                playerLabel.DOFade(1, 2).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnComplete);
                break;
            case "enemy":
                enemyLabel.gameObject.SetActive(true);
                enemyLabel.DOFade(1, 2).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnComplete);
                break;

        }
    }

    void OnMapPanelToggle(bool mapOpen) 
    {
        if (mapOpen)
        {
            if (inAnimation)
            {
                animationInterrupted = true;
            }
            DeactivateLabels();
        }
        else if (animationInterrupted) 
        {
            animationInterrupted = false;
            OnBeginOfTurn(lastTurn);
        }
    }

    void OnComplete()
    {
        inAnimation = false;
        DeactivateLabels();
    }

    private void DeactivateLabels()
    {
        playerLabel.gameObject.SetActive(false);
        enemyLabel.gameObject.SetActive(false);
    }
}
