using System.Collections;
using System.Collections.Generic;
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
        LoadingManager.Won = true;
        
        Cleanup.CleanupGame();
    }

    public void Populate(ScoreboardData scoreboard)
    {
        resultText.text = scoreboard.Outcome.ToUpper();
        finalScoreText.text = scoreboard.TotalScore.ToString();
        achievementfinalScoreText.text = scoreboard.TotalScore.ToString();
        expeditionTypeText.text = "Casual";

        // if there's no achievements, ignore them as none were earned
        if (scoreboard.Achievements == null) return;
        
        for (int i = 0; i < scoreboard.Achievements.Count; i++)
        {
            GameObject achievementInstance = Instantiate(scoreboardAchievementPrefab, achievementLayout.transform);
            ScoreboardAchievementManager achievementManager =
                achievementInstance.GetComponent<ScoreboardAchievementManager>();
            achievementManager.Populate(scoreboard.Achievements[i], i);
        }
    }
}