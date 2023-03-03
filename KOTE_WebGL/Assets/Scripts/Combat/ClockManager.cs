using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class ClockManager : MonoBehaviour
{
    [SerializeField]
    public bool ShowMilliSeconds = true;
    [SerializeField]
    public bool AlwaysShowMilliSeconds = false;
    [SerializeField]
    public bool ShowSeconds = true;
    [SerializeField]
    public bool AlwaysShowMinutes = true;
    [SerializeField]
    public bool AlwaysShowHours = false;
    [SerializeField]
    public bool AlwaysShowUnits = false;

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
        int hour = Mathf.FloorToInt(s / 3600);

        string unit = string.Empty;

        List<string> places = new List<string>();

        if (AlwaysShowHours || hour > 0) 
        {
            places.Add($"{hour:0}");
            unit = "h";
        }
        if (AlwaysShowMinutes || min > 0 && !AlwaysShowHours || hour > 0 && !AlwaysShowHours 
            || AlwaysShowMinutes && AlwaysShowHours)
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
        if (ShowSeconds || !AlwaysShowMinutes && !AlwaysShowHours)
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
            if (ShowMilliSeconds && (AlwaysShowMilliSeconds || (min == 0 && !AlwaysShowMinutes && hour == 0)))
            {
                secString += $".{millis:000}";
            }
            places.Add(secString);
            unit = "s";
        }

        return (Seconds < 0 ? "-" : "") + string.Join(":", places) + (places.Count == 1 || AlwaysShowUnits ? unit : "");
    }
}
