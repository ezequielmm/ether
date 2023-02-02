using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowConsole : MonoBehaviour
{
    public int timesClicked;
    public int timesNeeded = 10;

    public void OnClicked()
    {
        timesClicked += 1;
        if (timesClicked == timesNeeded)
        {
            GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
            timesClicked = 0;
        }
    }

    private void OnMouseExit()
    {
        timesClicked = 0;
    }
}
