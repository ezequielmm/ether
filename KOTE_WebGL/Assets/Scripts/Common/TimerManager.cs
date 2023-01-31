using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField]
    ClockManager clock;

    private float startTime = 0;
    private static float previousTime = 0;

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

    
    void Start()
    {
        startTime = Time.time;
    }

    private void OnDestroy()
    {
        previousTime = timePassed;
    }


    void Update()
    {
        timePassed = (Time.time - startTime) + previousTime;
    }
}
