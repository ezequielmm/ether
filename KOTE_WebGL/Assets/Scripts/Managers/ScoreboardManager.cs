using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ScoreboardManager : SingleTon<ScoreboardManager>
{
    [SerializeField] ScoreboardPanelManager Scoreboard;
    [SerializeField] LootboxPanelManager Lootbox;

    [SerializeField] string ExpiredLootMessage = $"The contest has ended before you had the chance to beat the boss. " +
                                                 $"As such, all loot has been forfeited.";
    void Start()
    {
        if (Scoreboard == null)
        {
            Scoreboard = GetComponent<ScoreboardPanelManager>();
        }

        if (Lootbox == null)
        {
            Lootbox = GetComponent<LootboxPanelManager>();
        }

        ToggleLootPanel(false);
        ToggleScorePanel(false);
    }

    public void ToggleScorePanel(bool enable)
    {
        Scoreboard.TogglePanel(enable);
        if (enable)
            GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    public void ToggleLootPanel(bool enable)
    {
        Lootbox.TogglePanel(enable);
        if (enable)
            GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    public async void UpdateAndShow(GameStatuses newGameStatus)
    {
        Debug.Log("[ScoreboardManager] Updating Score");
        var score = await FetchData.Instance.GetExpeditionScore();
        ShowScore(score, newGameStatus);
    }

    public void ShowScore(ScoreboardData scoreboardData, GameStatuses newGameStatus)
    {
        Debug.Log("[ScoreboardManager] Showing Score");
        Scoreboard.Populate(scoreboardData, newGameStatus);
        Lootbox.Populate(scoreboardData != null ? scoreboardData.VictoryItems : new List<VictoryItems>(), newGameStatus);
        
        if (scoreboardData == null || scoreboardData.VictoryItems.Count == 0)
        {
            ToggleScorePanel(true);
            if (scoreboardData != null && scoreboardData.NotifyNoLoot)
            {
                GameManager.instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(ExpiredLootMessage,
                    () => { }, () => { }, new[] { "It was fun while it lasted.", null });
            }
        }
        else
        {
            ToggleLootPanel(true);
        }
    }

    public void HideScore()
    {
        Debug.Log("[ScoreboardManager] Hiding Score");
        ToggleScorePanel(false);
        ToggleLootPanel(false);
    }
}

[Serializable]
public class ScoreboardData
{
    [JsonProperty("outcome")] public string Outcome;
    [JsonProperty("expeditionType")] public string ExpeditionType;
    [JsonProperty("totalScore")] public int TotalScore;
    [JsonProperty("achievements")] public List<Achievement> Achievements = new();
    [JsonProperty("notifyNoLoot")] public bool NotifyNoLoot;
    [JsonProperty("lootbox")] public List<GearItemData> Lootbox = new();
    [JsonProperty("rewards")] public List<RewardsLoot> Rewards = new();
    [JsonProperty("victoryItems")] public List<VictoryItems> VictoryItems = new();
}

[Serializable]
public class Achievement
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("score")] public int Score;
}