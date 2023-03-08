using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContestCountdownManager : MonoBehaviour
{
    [SerializeField]
    ClockManager countDownTimer;
    ContestManager contest;

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
    }

    private void Update()
    {
        countDownTimer.TotalSeconds = Mathf.Max(0, (float)contest.TimeUntilEnd.TotalSeconds);   
    }
}
