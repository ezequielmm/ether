using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ContestCountdownManager : MonoBehaviour
{
    [SerializeField]
    ClockManager countDownTimer;
    ContestManager contest;
    [SerializeField] string inContestBlurb = "until end of contest ({hour}:{minute} UTC)";
    [SerializeField] string noContestBlurb = "to join the contest ({hour}:{minute} UTC)";

    [SerializeField]
    TMP_Text CountdownBlurb;

    [SerializeField] private Leaderboard leaderboard;
    
    private void Awake()
    {
        contest = ContestManager.Instance;
    }
    private void Start()
    {
        contest.OnContestStarted.AddListener(EnableTimer);
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.AddListener(
            value =>
            ((Action)(value ? DisableTimer : EnableTimer))()
        );
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener(
            value =>
                ((Action)(value ? DisableTimer : EnableTimer))()
        );
        leaderboard.OnLeaderboardShow.AddListener(
            value =>
                ((Action)(value ? DisableTimer : EnableTimer))()
        );
    }

    private void OnEnable()
    {
        ToggleTimer(contest.HasContest);
    }

    public void EnableTimer() 
    {
        ToggleTimer(true);
    }
    public void DisableTimer()
    {
        ToggleTimer(false);
    }

    public void ToggleTimer(bool active)
    {
        countDownTimer.gameObject.SetActive(active);
        if(active) 
        {
            DateTime endOfContest = GetEndTime();
            CountdownBlurb.text = GenerateText(endOfContest);
        }
    }

    private string GenerateText(DateTime time)
    {
        string textBlurb = noContestBlurb;
        if (contest.InContest)
        {
            textBlurb = inContestBlurb;
        }
        string hour = $"{time.Hour:00}";
        string minute = $"{time.Minute:00}";
        return textBlurb.Replace("{hour}", hour).Replace("{minute}", minute);
    }

    private DateTime GetEndTime()
    {
        if (contest.InContest)
        {
            return contest.ContestEndTimeUtc;
        }
        else
        {
            return contest.LastSubmissionTimeUtc;
        }
    }
    
    private double GetTimeLeft()
    {
        if (contest.InContest)
        {
            return contest.TimeUntilEnd.TotalSeconds;
        }
        else
        {
            return contest.TimeUntilLastSubmission.TotalSeconds;
        }
    }

    private void Update()
    {
        countDownTimer.TotalSeconds = Mathf.Max(0, (float)GetTimeLeft());   
    }
    
    public enum CounterEndSource
    {
        EndAtEndOfSubmissions,
        EndAtEndOfContest
    }
}
