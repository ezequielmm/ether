using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
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
        GameManager.Instance.EVENT_SHOW_SCOREBOARD.AddListener(OnShowScoreboard);
    }

    public void OnShowAchievementsButton()
    {
        achievementPanel.SetActive(!achievementPanel.activeSelf);
    }

    public void OnContinueButton()
    {
        GameManager.Instance.LoadScene(inGameScenes.MainMenu);
    }

    private void OnShowScoreboard(SWSM_ScoreboardData scoreboardData)
    {
        // if the scoreboard data is null, that means there was an error, so just show the main menu button
        if (scoreboardData == null)
        {
            scorePanel.SetActive(false);
            gameOverContainer.SetActive(true);
            return;
        }
        
        Populate(scoreboardData);
        gameOverContainer.SetActive(true);
    }

    private void Populate(SWSM_ScoreboardData data)
    {
        resultText.text = data.data.outcome;
        finalScoreText.text = data.data.totalScore.ToString();
        achievementfinalScoreText.text = data.data.totalScore.ToString();
        expeditionTypeText.text = string.IsNullOrEmpty(data.data.expeditionType) ? data.data.expeditionType : "Casual Mode";

        // if there's no achievements, ignore them as none were earned
        if (data.data.achievements == null) return;
        
        for (int i = 0; i < data.data.achievements.Length; i++)
        {
            GameObject achievementInstance = Instantiate(scoreboardAchievementPrefab, achievementLayout.transform);
            ScoreboardAchievementManager achievementManager =
                achievementInstance.GetComponent<ScoreboardAchievementManager>();
            achievementManager.Populate(data.data.achievements[i], i);
        }
    }
}