using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusIcon : IconMap<STATUS>
{
    // Start is called before the first frame update
    void Awake()
    {
        this.tooltipSpeed = GameSettings.INTENT_TOOLTIP_SPEED;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
