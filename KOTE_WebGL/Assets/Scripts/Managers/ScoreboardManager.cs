using KOTE.UI.Armory;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardManager : SingleTon<ScoreboardManager>
{
    [SerializeField]
    ScoreboardPanelManager Scoreboard;
    [SerializeField]
    LootboxPanelManager Lootbox;

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

    public async void UpdateScore()
    {
        ScoreData = await FetchData.Instance.GetExpeditionScore();
        if (ScoreData == null)
        {
            HideScore();
            return;
        }
        ShowScore();
    }

    public void ShowScore()
    {
        Scoreboard.Populate(ScoreData);
        Lootbox.Populate(ScoreData.Lootbox);
        if (ScoreData.Lootbox.Count == 0)
        {
            ToggleLootPanel(true);
        }
        else 
        {
            ToggleScorePanel(true);
        }
    }

    public void HideScore()
    {
        ToggleScorePanel(false);
        ToggleLootPanel(false);
    }
}

[Serializable]
public class SWSM_ScoreboardData
{
    public ScoreboardData data;
}

[Serializable]
public class ScoreboardData
{
    [JsonProperty("outcome")]
    public string Outcome;
    [JsonProperty("expeditionType")]
    public string ExpeditionType;
    [JsonProperty("totalScore")]
    public int TotalScore;
    [JsonProperty("achievements")]
    public List<Achievement> Achievements = new();
    [JsonProperty("validContestResults")]
    public bool ValidContestResults;
    [JsonProperty("lootbox")]
    public List<GearItemData> Lootbox = new();
}

[Serializable]
public class Achievement
{
    [JsonProperty("name")]
    public string Name;
    [JsonProperty("score")]
    public int Score;
}