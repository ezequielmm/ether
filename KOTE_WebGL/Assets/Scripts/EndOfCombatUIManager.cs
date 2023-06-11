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

    public TextMeshProUGUI messageLabel;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameStatusChange);
        GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT.AddListener(OnShowOverlayText);
        GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT_WITH_ON_COMPLETE.AddListener(OnShowOverlayText);
        DeactivateLabels();
    }

    private void OnGameStatusChange(GameStatuses data)
    {
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

    private void OnShowOverlayText(string text)
    {
        messageLabel.text = text;
        messageLabel.DOFade(1, 2).SetDelay(GameSettings.VICTORY_LABEL_ANIMATION_DELAY).From(0)
            .SetLoops(2, LoopType.Yoyo).OnComplete(() => { messageLabel.gameObject.SetActive(false); })
            .OnStart(() => { messageLabel.gameObject.SetActive(true); });
    }

    private void OnShowOverlayText(string text, Action OnComplete)
    {
        messageLabel.text = text;
        messageLabel.DOFade(1, 2).SetDelay(GameSettings.VICTORY_LABEL_ANIMATION_DELAY).From(0)
            .SetLoops(2, LoopType.Yoyo).OnComplete(() =>
            {
                messageLabel.gameObject.SetActive(false);
                OnComplete.Invoke();
            })
            .OnStart(() => { messageLabel.gameObject.SetActive(true); });
    }

    void OnGameOverComplete()
    {
        DeactivateLabels();
    }

    void OnVictoryComplete()
    {
        DeactivateLabels();
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
    }

    void ReloadScene()
    {
        GameManager.Instance.LoadScene(inGameScenes.Expedition);
    }

    void LoadMainMenu()
    {
     
    }


    private void DeactivateLabels()
    {
        gameoverLabel.gameObject.SetActive(false);
        victoryLabel.gameObject.SetActive(false);
    }
}