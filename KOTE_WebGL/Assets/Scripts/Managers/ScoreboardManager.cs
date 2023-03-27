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

    ScoreboardData ScoreData;

    protected override void Awake()
    {
        base.Awake();
    }

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
    }

    public void ToggleLootPanel(bool enable)
    {
        Lootbox.TogglePanel(enable);
    }

    public async void UpdateAndShow()
    {
        Debug.Log("[ScoreboardManager] Updating Score");
        await UpdateScore();

        if (ScoreData == null)
            HideScore();
        else
            ShowScore();
    }

    public async UniTask UpdateScore()
    {
        ScoreData = await FetchData.Instance.GetExpeditionScore();
    }

    public void ShowScore()
    {
        Debug.Log("[ScoreboardManager] Showing Score");
        Scoreboard.Populate(ScoreData);
        Lootbox.Populate(ScoreData.Lootbox);
        if (ScoreData.Lootbox.Count == 0)
        {
            ToggleScorePanel(true);
            if (ScoreData.NotifyNoLoot)
            {
                GameManager.instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(ExpiredLootMessage,
                    () => { }, () => { }, new[] { "It was fun while it lasted.", null });
            }
        }
        else
        {
            GameManager.Instance.ShowArmory = true;
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
}

[Serializable]
public class Achievement
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("score")] public int Score;
}