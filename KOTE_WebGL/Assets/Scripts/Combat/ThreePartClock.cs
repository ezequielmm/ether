using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThreePartClock : ClockManager
{
    [SerializeField]
    TMP_Text HourText;
    [SerializeField]
    TMP_Text MinuteText;
    [SerializeField]
    TMP_Text SecondText;

    [SerializeField]
    TMP_Text HourLabelText;
    [SerializeField]
    TMP_Text MinuteLabelText;
    [SerializeField]
    TMP_Text SecondLabelText;

    protected override void UpdateClock()
    {
        HourText.text = $"{Hours}";
        MinuteText.text = $"{Minutes:00}";
        SecondText.text = $"{Seconds:00}";

        HourLabelText.text = $"Hour{Plural(Hours)}";
        MinuteLabelText.text = $"Minute{Plural(Minutes)}";
        SecondLabelText.text = $"Second{Plural(Seconds)}";
    }

    private string Plural(int amount) 
    {
        return amount != 1 ? "s" : "";
    }
}