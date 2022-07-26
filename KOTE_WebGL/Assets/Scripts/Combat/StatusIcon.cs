using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusIcon : IconMap<STATUS>
{
    void Awake()
    {
        this.tooltipSpeed = GameSettings.INTENT_TOOLTIP_SPEED;
    }
    public void SetValue(int value) 
    {
        if (value <= 0)
        {
            SetDisplayText(string.Empty);
        }
        else 
        {
            SetDisplayText(value.ToString());
        }
    }
}
