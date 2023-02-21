using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIDisableGameWhenOpen : MonoBehaviour
{
    private static int windowsOpenCount;
    private bool isBlocking = false;
    private void OnDisable()
    {
        if (isBlocking)
        {
            EnableClick();
        }
        isBlocking = false;
    }
    private void OnEnable()
    {
        if (!isBlocking)
        {
            DisableClick();
        }
        isBlocking = true;
    }

    private void OnDestroy()
    {
        if (isBlocking) 
        {
            isBlocking = false;
            EnableClick();
        }
    }

    public static void DisableClick() 
    {
        windowsOpenCount++;
        if (windowsOpenCount > 0)
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
        }
    }

    public static void EnableClick()
    {
        windowsOpenCount--;
        if (windowsOpenCount == 0)
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(false);
        }
    }

}
