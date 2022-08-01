using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIDisableGameWhenOpen : MonoBehaviour
{
    [SerializeField]
    bool DisableGameOnOpen = true;
    [SerializeField]
    bool EnableGameOnClose = true;

    private static int windowsOpenCount;
    private void OnDisable()
    {
        if (EnableGameOnClose)
        {
            windowsOpenCount--;
        }
        if (windowsOpenCount == 0) 
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(false);
        }
    }
    private void OnEnable()
    {
        if (DisableGameOnOpen)
        {
            windowsOpenCount++;
        }
        if (windowsOpenCount > 0) 
        {
            GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
        }
    }
}
