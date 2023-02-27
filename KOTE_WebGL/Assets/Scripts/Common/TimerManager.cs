using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField]
    ClockManager clock;

    private float startTimeSeconds = 0;
    private static float previousTimeSeconds = 0;

    private float timePassed
    {
        get
        {
            return clock.Seconds;
        }
        set
        {
            clock.Seconds = value;
        }
    }

    private void Awake()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(TimerUpdated);
    }

    private void TimerUpdated(PlayerStateData playerState) 
    {
        DateTime timerBegining = playerState.data.expeditionCreatedAt;
        DateTime now = DateTime.UtcNow;
        TimeSpan timePassed = now - timerBegining;
        previousTimeSeconds = (float)timePassed.TotalSeconds;
    }

    void Start()
    {
        startTimeSeconds = Time.time;
    }

    private void OnDestroy()
    {
        previousTimeSeconds = timePassed;
    }


    void Update()
    {
        timePassed = (Time.time - startTimeSeconds) + previousTimeSeconds;
    }
}
