using Cysharp.Threading.Tasks;
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
            ToggleLootPanel(true);
        }
        else 
        {
            ToggleScorePanel(true);
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
    [JsonProperty("outcome")]
    public string Outcome;
    [JsonProperty("expeditionType")]
    public string ExpeditionType;
    [JsonProperty("totalScore")]
    public int TotalScore;
    [JsonProperty("achievements")]
    public List<Achievement> Achievements = new();
    [JsonProperty("notifyNoLoot")]
    public bool NotifyNoLoot;
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