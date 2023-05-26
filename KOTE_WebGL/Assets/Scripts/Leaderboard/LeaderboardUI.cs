using System;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    private void Start()
    {
        //string ServerVersion = await FetchData.Instance.GetServerVersion();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
