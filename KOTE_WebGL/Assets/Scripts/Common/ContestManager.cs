using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContestManager : SingleTon<ContestManager>
{
    public UnityEvent OnNoMoreSubmissions = new UnityEvent();
    public UnityEvent OnContestEnded = new UnityEvent();
    public UnityEvent OnContestStarted = new UnityEvent();
    public bool HasContest { get; private set; } = false;
    bool reportedEndOfContest = true;

    public bool InContest { get; private set; } = false;

    public DateTime ContestEndTimeUtc { get; private set; }
    public DateTime LastSubmissionTimeUtc { get; private set; }
    public TimeSpan TimeUntilLastSubmission => LastSubmissionTimeUtc - DateTime.UtcNow;
    public TimeSpan TimeUntilEnd => ContestEndTimeUtc - DateTime.UtcNow;

    public ContestData ContestData => InContest ? CurrentContest : OngoingContest;
    
    private ContestData OngoingContest;
    private ContestData CurrentContest => UserDataManager.Instance.ContestData;

    private DateTime NextDay0AmUtc()
    {
        var time = DateTime.UtcNow;
        time = time.AddDays(1);
        return new DateTime(time.Year, time.Month, time.Day);
    }

    private DateTime Next6AmUtc() 
    {
        var time = DateTime.UtcNow.AddHours(-6);
        time = time.AddDays(1);
        return new DateTime(time.Year, time.Month, time.Day).AddHours(6);
    }

    public void SetNewContestEndTime(DateTime UtcDateTime)
    {
        reportedEndOfContest = false;
        ContestEndTimeUtc = UtcDateTime;
        OnContestStarted.Invoke();
    }
    
    public void SetNewContestSubmissionTime(DateTime UtcDateTime)
    {
        LastSubmissionTimeUtc = UtcDateTime;
    }

    public void SetContestTimes(DateTime StartTime, DateTime EndTime, DateTime SubmissionEndTime)
    {
        SetNewContestEndTime(EndTime);
        SetNewContestSubmissionTime(SubmissionEndTime);
    }

    void Start()
    {
        UserDataManager.Instance.ExpeditionStatusUpdated.AddListener(UpdateContestTimes);
        reportedEndOfContest = false;
        OnContestEnded.AddListener(ResetContestOnEnd);
        UpdateContestTimes();
    }

    private async UniTask CheckContestStatus() 
    {
        OngoingContest = await FetchData.Instance.GetOngoingContest();
        HasContest = true;
    }

    private async void ResetContestOnEnd() 
    {
        await CheckContestStatus();
        if (HasContest)
        {
            if(AuthenticationManager.Instance.Authenticated)
                await UserDataManager.Instance.UpdateExpeditionStatus();
            UpdateContestTimes();
        }
    }

    private async void UpdateContestTimes()
    {
        await UniTask.WaitUntil(() => ContestData != null);
        SetContestTimes(ContestData.StartTime, ContestData.EndTime,
            ContestData.SubmissionsUntilTime);
        InContest = UserDataManager.Instance.HasExpedition; // TODO: Get this info directly from the backend.
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
