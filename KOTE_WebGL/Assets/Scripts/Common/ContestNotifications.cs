using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContestNotifications : MonoBehaviour
{
    [SerializeField]
    List<int> TimesToDisplayWarningInSconds = new();
    int timeIndex = 0;

    [SerializeField]
    string WarningText = $"The contest will be ending in {{time}}.";

    [SerializeField]
    string EndOfContestText = $"The contest has ended! To continue playing, you'll have to return to the main menu and start a new expedition.";
    ContestManager contest;

    private void Awake()
    {
        contest = ContestManager.Instance;
        contest.OnContestEnded.AddListener(GiveContestEnded);
        TimesToDisplayWarningInSconds.Sort();
        TimesToDisplayWarningInSconds.Reverse();
        InitializeTimeWatching();
    }

    private void InitializeTimeWatching() 
    {
        int currentSecondCount = (int)contest.TimeUntilEnd.TotalSeconds;
        for(timeIndex = 0; timeIndex < TimesToDisplayWarningInSconds.Count; timeIndex++) 
        {
            int waitingForSecondToPass = TimesToDisplayWarningInSconds[timeIndex];
            if (currentSecondCount >= waitingForSecondToPass) 
            {
                return;
            }
        }

    }
    
    private void Update()
    {
        if (timeIndex >= TimesToDisplayWarningInSconds.Count || timeIndex < 0)
            return;
        int waitingForSecondToPass = TimesToDisplayWarningInSconds[timeIndex];
        int currentSecondCount = (int)contest.TimeUntilEnd.TotalSeconds;
        if (currentSecondCount < waitingForSecondToPass) 
        {
            timeIndex++;
            GiveWarning(waitingForSecondToPass);
        }
    }

    public void GiveWarning(int timeLeft) 
    {
        string warning = FormatTimeInMessage(timeLeft, WarningText);
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(warning, () => { }, () => { },
            new string[] { "Close", null });
    }

    public void GiveContestEnded()
    {
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.Invoke(EndOfContestText, () =>
            {
                Debug.Log("GiveContestEnded");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UserDataManager.Instance.ClearExpedition();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                GameManager.Instance.LoadScene(inGameScenes.MainMenu);
            }, () => { },
            new string[] { "Back to Main Menu", "Continue (No Lootbox)" });
    }

    private string FormatTimeInMessage(int seconds, string text) 
    {
        TimeSpan time = new TimeSpan(0,0,seconds);
        List<string> timeStrings = new List<string>();
        if (time.Hours > 0) 
        {
            timeStrings.Add($"{time.Hours} Hour{(time.Hours == 1 ? "" : "s")}");
        }
        if (time.Minutes > 0)
        {
            timeStrings.Add($"{time.Minutes} Minute{(time.Minutes == 1 ? "" : "s")}");
        }
        string timeString = string.Join(", ", timeStrings);
        return text.Replace("{time}", timeString) ;
    }
}
