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

    [SerializeField]
    TMP_Text CountdownBlurb;

    private void Start()
    {
        contest = ContestManager.Instance;
        contest.OnContestStarted.AddListener(EnableTimer);
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
            DateTime endOfContest = contest.ContestEndTimeUtc;
            CountdownBlurb.text = $"until end of contest ({endOfContest.Hour:00}:{endOfContest.Minute:00} UTC)";
        }
    }

    private void Update()
    {
        countDownTimer.TotalSeconds = Mathf.Max(0, (float)contest.TimeUntilEnd.TotalSeconds);   
    }
}
