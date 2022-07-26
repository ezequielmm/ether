using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using DG.Tweening;

public class IntentIcon : IconMap<ENEMY_INTENT>
{
    // Start is called before the first frame update
    void Awake()
    {
        tooltipSpeed = GameSettings.INTENT_TOOLTIP_SPEED;
    }

    public void SetValue(int value = 0, int times = 1) 
    {
        if (value * times <= 0)
        {
            SetDisplayText("");
        }
        else
        {
            SetDisplayText($"{value}{(times > 1 ? $"x{times}" : "")}");
        }
    }
}
