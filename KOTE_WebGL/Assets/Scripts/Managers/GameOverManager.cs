using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverContainer;
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
        GameManager.Instance.EVENT_GAME_OVER.AddListener(OnGameOver);
    }

    public void OnShowAchievementsButton()
    {
        achievementPanel.SetActive(!achievementPanel.activeSelf);
    }

    public void OnContinueButton()
    {
        GameManager.Instance.LoadScene(inGameScenes.MainMenu);
    }

    private void OnGameOver(SWSM_GameOverData gameOverData)
    {
        Populate(gameOverData);
        gameOverContainer.SetActive(true);
    }

    private void Populate(SWSM_GameOverData data)
    {
        resultText.text = data.data.result;
        finalScoreText.text = data.data.finalScore.ToString();
        achievementfinalScoreText.text = data.data.finalScore.ToString();
        expeditionTypeText.text = string.IsNullOrEmpty(data.data.expeditionType) ? data.data.expeditionType : "CasualMode";
        Achievement[] achievements = new[]
        {
            new Achievement
            {
                name = "test",
                score = 1
            },
            new Achievement
            {
                name = "test",
                score = 2
            },
            new Achievement
            {
                name = "test",
                score = 3
            },
            new Achievement
            {
                name = "test",
                score = 4
            }
        };

        for (int i = 0; i < achievements.Length; i++)
        {
            GameObject achievementInstance = Instantiate(scoreboardAchievementPrefab, achievementLayout.transform);
            ScoreboardAchievementManager achievementManager =
                achievementInstance.GetComponent<ScoreboardAchievementManager>();
            achievementManager.Populate(achievements[i], i);
        }
    }
}