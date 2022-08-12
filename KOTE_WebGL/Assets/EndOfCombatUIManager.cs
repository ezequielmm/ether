using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class EndOfCombatUIManager : MonoBehaviour
{
    public TextMeshProUGUI gameoverLabel;
    public TextMeshProUGUI victoryLabel;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameStatusChange);
        DeactivateLabels();
    }

    private void OnGameStatusChange(GameStatuses data)
    {
        switch (data)
        {
            case GameStatuses.GameOver:
                gameoverLabel.gameObject.SetActive(true);
                gameoverLabel.DOFade(1, 2).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnComplete); 
                break;
            case GameStatuses.RewardsPanel:
               
                victoryLabel.DOFade(1, 2).SetDelay(GameSettings.VICTORY_LABEL_ANIMATION_DELAY).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnComplete).OnStart(()=> { victoryLabel.gameObject.SetActive(true); }); 
                break;
        }
    }

    void OnComplete()
    {
        DeactivateLabels();
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Continue", ReloadScene);
    }

    void ReloadScene()
    {
        GameManager.Instance.LoadScene(inGameScenes.Expedition);
    }


    private void DeactivateLabels()
    {
        gameoverLabel.gameObject.SetActive(false);
        victoryLabel.gameObject.SetActive(false);
    }
}
