using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestCountdownManager : MonoBehaviour
{
    [SerializeField]
    ClockManager countDownTimer;
    DateTime contestEndTimeUtc;

    TimeSpan TimeUntilEnd => contestEndTimeUtc - DateTime.UtcNow;

    void Start()
    {
        contestEndTimeUtc = DateTime.UtcNow;
        contestEndTimeUtc = contestEndTimeUtc.AddDays(1);
        contestEndTimeUtc = new DateTime(contestEndTimeUtc.Year, contestEndTimeUtc.Month, contestEndTimeUtc.Day);
    }

    private void Update()
    {
        countDownTimer.TotalSeconds = (float)TimeUntilEnd.TotalSeconds;
    }
}
