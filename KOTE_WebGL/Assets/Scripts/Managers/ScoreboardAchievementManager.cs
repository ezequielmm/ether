using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardAchievementManager : MonoBehaviour
{
    public GameObject background;
    public TMP_Text achievementName;
    public TMP_Text score;

    public void Populate(Achievement achievement, int index)
    {
        achievementName.text = achievement.Name;
        score.text = $"+ {achievement.Score}";
        if (index % 2 == 0)
        {
            background.SetActive(false);
        }
    }
}
