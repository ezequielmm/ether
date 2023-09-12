using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardPanelManager : MonoBehaviour
{
    public GameObject gameOverContainer;
    public GameObject scorePanel;
    public GameObject achievementPanel;
    public GameObject achievementLayout;
    public GameObject scoreboardAchievementPrefab;
    [Space] public TMP_Text resultText;
    public TMP_Text finalScoreText;
    public TMP_Text achievementfinalScoreText;
    public TMP_Text expeditionTypeText;
    public TMP_Text continueButtonText;
    private GameStatuses populatedWithStatus;

    public void Start()
    {
        gameOverContainer.SetActive(false);
        achievementPanel.SetActive(false);
    }

    public void TogglePanel(bool enable)
    {
        gameOverContainer.SetActive(enable);
    }

    public void OnShowAchievementsButton()
    {
        achievementPanel.SetActive(!achievementPanel.activeSelf);
    }

    public void OnContinueButton()
    {
        if (populatedWithStatus == GameStatuses.ScoreBoard)
        {
            LoadingManager.Won = true;
            Cleanup.CleanupGame();
        }
        else if (populatedWithStatus == GameStatuses.ScoreBoardAndNextAct)
        {
            GameManager.Instance.LoadSceneNewTest();
        }
    }

    public void Populate(ScoreboardData scoreboard, GameStatuses newGameStatus)
    {
        resultText.text = scoreboard != null ? scoreboard.Outcome.ToUpper() : "???";
        finalScoreText.text = scoreboard != null ? scoreboard.TotalScore.ToString() : "???";
        achievementfinalScoreText.text = scoreboard != null ? scoreboard.TotalScore.ToString() : "???";
        expeditionTypeText.text = "Casual";

        continueButtonText.text = newGameStatus == GameStatuses.ScoreBoard ? "Main Menu" : "Continue";
        
        populatedWithStatus = newGameStatus;

        // if there's no achievements, ignore them as none were earned
        if (scoreboard == null || scoreboard.Achievements == null) return;
        
        for (int i = 0; i < scoreboard.Achievements.Count; i++)
        {
            GameObject achievementInstance = Instantiate(scoreboardAchievementPrefab, achievementLayout.transform);
            ScoreboardAchievementManager achievementManager =
                achievementInstance.GetComponent<ScoreboardAchievementManager>();
            achievementManager.Populate(scoreboard.Achievements[i], i);
        }
    }
}