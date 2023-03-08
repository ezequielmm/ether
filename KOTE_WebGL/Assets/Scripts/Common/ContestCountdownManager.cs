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
    }

    private void Update()
    {
        countDownTimer.TotalSeconds = Mathf.Max(0, (float)contest.TimeUntilEnd.TotalSeconds);   
    }
}
