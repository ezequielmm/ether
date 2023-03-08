using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContestManager : SingleTon<ContestManager>
{
    public UnityEvent OnContestEnded = new UnityEvent();
    public UnityEvent OnContestStarted = new UnityEvent();
    public bool HasContest { get; private set; } = false;
    bool reportedEndOfContest = true;

    public DateTime ContestEndTimeUtc { get; private set; }
    public TimeSpan TimeUntilEnd => ContestEndTimeUtc - DateTime.UtcNow;

    private DateTime NextDayUtc() 
    {
        var time = DateTime.UtcNow;
        time = time.AddDays(1);
        return new DateTime(time.Year, time.Month, time.Day);
    }

    public void SetNewContestTime(DateTime UtcDateTime)
    {
        reportedEndOfContest = false;
        ContestEndTimeUtc = UtcDateTime;
        OnContestStarted.Invoke();
    }

    void Start()
    {
        ResetContestOnEnd();
        reportedEndOfContest = false;
        OnContestEnded.AddListener(ResetContestOnEnd);
    }

    private async UniTask CheckContestStatus() 
    {
        HasContest = true;
    }

    private async void ResetContestOnEnd() 
    {
        await CheckContestStatus();
        if (HasContest)
        {
            SetNewContestTime(NextDayUtc());
        }
    }

    private void Update()
    {
        if(TimeUntilEnd.TotalSeconds <= 0 && !reportedEndOfContest) 
        {
            Debug.Log($"$[ContestManager] Contest has ended!");
            reportedEndOfContest = true;
            OnContestEnded.Invoke();
        }
    }
}
