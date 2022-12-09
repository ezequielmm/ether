using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserActivityMonitor : SingleTon<UserActivityMonitor>
{
    [Tooltip("The time in minutes a player can be AFK before triggering a logout")]
    public int AfkTime = 5;

    [Tooltip("the time in seconds between afk checks")]
    public float AfkCheckInterval = 1;

    private int currentSeconds;
    private int afkSeconds;
    private bool resetTimer;
    private Coroutine afkTimer;

    public void Start()
    {
        afkSeconds = AfkTime * 60;
        afkTimer = StartCoroutine(AfkLogoutTimer());
    }

    public void OnDestroy()
    {
        if (afkTimer != null)
        {
            StopCoroutine(afkTimer);
        }
    }

    private IEnumerator AfkLogoutTimer()
    {
        while (true)
        {
            if (resetTimer)
            {
                currentSeconds = 0;
                resetTimer = false;
            }

            currentSeconds++;
            yield return new WaitForSeconds(AfkCheckInterval);

            if (currentSeconds > afkSeconds)
            {
                Debug.Log("[UserActivityMonitor] Afk logout activated");
                // if the timer is up, return the player to the main menu
                string token = PlayerPrefs.GetString("session_token");
                if (!string.IsNullOrEmpty(token))
                {
                    GameManager.Instance.EVENT_REQUEST_LOGOUT.Invoke(token);
                }
            }
        }
    }

    // THIS MESSAGE IS CALLED FROM JAVASCRIPT WHEN THE WEBPAGE IS RUNNING
    private void GetMessageFromJs()
    {
        resetTimer = true;
    }
}