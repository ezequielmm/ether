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
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener(OnBeginOfTurn);
        DeactivateLabels();
    }

    
    void OnBeginOfTurn(string who)
    {
        if(firstPlay)
        {
            firstPlay = false;
            return;
        }
        Debug.Log("[OnBeginOfTurn]who: " + who);
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

    void OnComplete()
    {
        //DeactivateLabels();
    }

    private void DeactivateLabels()
    {
        playerLabel.gameObject.SetActive(false);
        enemyLabel.gameObject.SetActive(false);
    }
}
