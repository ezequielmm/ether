using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleClockManager : ClockManager
{
    [SerializeField]
    TMP_Text clockText;

    void Start()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TMP_Text>();
        }
    }

    protected override void UpdateClock()
    {
        if (clockText != null)
        {
            clockText.text = ToString();
        }
    }
}