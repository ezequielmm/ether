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

    public float TimePassed => (Time.time - startTimeSeconds) + previousTimeSeconds;

    private void Awake()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);
    }

    private void OnPlayerStatusUpdate(PlayerStateData playerState) 
    {
        SetTimerStartTime(playerState.data.expeditionCreatedAt);
    }

    public void SetTimerStartTime(DateTime startTimeUTC) 
    {
        TimeSpan timePassed = DateTime.UtcNow - startTimeUTC;
        previousTimeSeconds = (float)timePassed.TotalSeconds;
    }

    void Start()
    {
        startTimeSeconds = Time.time;
    }

    private void OnDestroy()
    {
        previousTimeSeconds = TimePassed;
    }

    public void Reset()
    {
        startTimeSeconds = 0;
        previousTimeSeconds = 0;
        Start();
    }

    void Update()
    {
        clock.Seconds = TimePassed;
    }
}
