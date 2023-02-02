using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    [SerializeField]
    bool showMilliSeconds = true;
    [SerializeField]
    bool forceMilliSeconds = false;
    [SerializeField]
    bool showSeconds = true;
    [SerializeField]
    bool forceMinutes = true;
    [SerializeField]
    bool forceHours = false;
    [SerializeField]
    bool forceUnits = false;

    public float Seconds;

    [SerializeField]
    TMP_Text clockText;

    void Start()
    {
        if(clockText == null) 
        {
            clockText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (clockText != null)
        {
            clockText.text = ToString();
        }
    }

    public override string ToString()
    {
        var s = Mathf.Abs(Seconds);

        int millis = Mathf.FloorToInt(s*1000 % 1000);
        int sec = Mathf.FloorToInt(s % 60);
        int min = Mathf.FloorToInt((s / 60) % 60);
        int hour = Mathf.FloorToInt((s / 3600) % 60);

        string unit = string.Empty;

        List<string> places = new List<string>();
        if (forceHours || hour > 0) 
        {
            places.Add($"{hour:0}");
            unit = "h";
        }
        if (forceMinutes || min > 0)
        {
            string minString = string.Empty;
            if (places.Count == 0)
            {
                minString = $"{min:0}";
            }
            else
            {
                minString = $"{min:00}";
            }
            places.Add(minString);
            unit = "m";
        }
        if (showSeconds || !forceMinutes && !forceHours)
        {
            string secString = string.Empty;
            if (places.Count == 0)
            {
                secString = $"{sec:0}";
            }
            else 
            {
                secString = $"{sec:00}";
            }
            if (showMilliSeconds && (forceMilliSeconds || (min == 0 && !forceMinutes && hour == 0)))
            {
                secString += $".{millis:000}";
            }
            places.Add(secString);
            unit = "s";
        }

        return (Seconds < 0 ? "-" : "") + string.Join(":", places) + (places.Count == 1 || forceUnits ? unit : "");
    }
}
