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

    GameStatuses lastState;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameStatusChange);
        DeactivateLabels();
    }

    private void OnGameStatusChange(GameStatuses data)
    {
        lastState = data;
        switch (data)
        {
            case GameStatuses.GameOver:
                // Throw up click blocker
                GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
                gameoverLabel.gameObject.SetActive(true);
                gameoverLabel.DOFade(1, 2).From(0).SetLoops(2, LoopType.Yoyo).OnComplete(OnGameOverComplete);
                break;
            case GameStatuses.RewardsPanel:
                // Throw up click blocker
                GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
                victoryLabel.DOFade(1, 2).SetDelay(GameSettings.VICTORY_LABEL_ANIMATION_DELAY).From(0)
                    .SetLoops(2, LoopType.Yoyo).OnComplete(OnVictoryComplete).OnStart(() =>
                    {
                        victoryLabel.gameObject.SetActive(true);
                    });
                break;
        }
    }

    void OnGameOverComplete()
    {
        DeactivateLabels();
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.Invoke("Continue", ReloadScene);
    }

    void OnVictoryComplete()
    {
        DeactivateLabels();
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
    }

    void ReloadScene()
    {
        if (lastState == GameStatuses.GameOver) 
        {
            GameManager.Instance.LoadScene(inGameScenes.MainMenu);
            return;
        }
        GameManager.Instance.LoadScene(inGameScenes.Expedition);
    }


    private void DeactivateLabels()
    {
        gameoverLabel.gameObject.SetActive(false);
        victoryLabel.gameObject.SetActive(false);
    }
}