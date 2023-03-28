using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
#if !UNITY_EDITOR
        afkTimer = StartCoroutine(AfkLogoutTimer());
#endif
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
                GameManager.Instance.EVENT_HIDE_WARNING_MESSAGE.Invoke();
            }


            currentSeconds++;
            yield return new WaitForSeconds(AfkCheckInterval);
            // if the player isn't logged in, dont do anything
            string token = AuthenticationManager.Instance.GetSessionToken();
            if (string.IsNullOrEmpty(token))
            {
                continue;
            }

            // if the timer is up, return the player to the main menu
            if (currentSeconds > afkSeconds)
            {
                Debug.Log("[UserActivityMonitor] Afk logout activated");

                AuthenticationManager.Instance.Logout();
            }

            if (afkSeconds - currentSeconds <= 10)
            {
                GameManager.Instance.EVENT_SHOW_WARNING_MESSAGE.Invoke(
                    $"Logging out for inactivity in {afkSeconds - currentSeconds} seconds");
            }
        }
    }

    // THIS MESSAGE IS CALLED FROM JAVASCRIPT WHEN THE WEBPAGE IS RUNNING
    private void GetMessageFromJs()
    {
        resetTimer = true;
    }
}