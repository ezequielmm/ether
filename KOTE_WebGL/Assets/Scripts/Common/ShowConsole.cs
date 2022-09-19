using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowConsole : MonoBehaviour
{
    public int timesClicked;
    private int timesNeeded = 10;

    public void OnClicked()
    {
        timesClicked += 1;
        if (timesClicked == 10)
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
