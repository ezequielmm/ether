using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIDisableGameWhenOpen : MonoBehaviour
{
    private static int windowsOpenCount;
    private void OnDisable()
    {
        windowsOpenCount--;
        if (windowsOpenCount == 0) 
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(false);
        }
    }
    private void OnEnable()
    {
        windowsOpenCount++;
        if (windowsOpenCount > 0) 
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
        }
    }
}
