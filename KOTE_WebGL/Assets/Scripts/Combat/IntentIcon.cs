using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;

public class IntentIcon : IconMap<ENEMY_INTENT>
{
    void Awake()
    {
        tooltipSpeed = GameSettings.INTENT_TOOLTIP_SPEED;
    }

    public void SetValue(int value = 0, int times = 1, string title = null) 
    {
        if (!string.IsNullOrEmpty(title))
            if (value * times <= 0)
                SetDisplayText(string.IsNullOrEmpty(title) ? "" : title);
            else
                SetDisplayText(string.IsNullOrEmpty(title) ? "" : $"{title}: " + $"{value}{(times > 1 ? $"x{times}" : "")}");
        else
        {
            if(value == 0)
                SetDisplayText("");
            else
                SetDisplayText($"{value}{(times > 1 ? $"x{times}" : "")}");
        }
    }
}
