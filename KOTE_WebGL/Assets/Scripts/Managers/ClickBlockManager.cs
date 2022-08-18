using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickBlockManager : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener(onToggle);
        Disable();
    }

    private void onToggle(bool? newValue) 
    {
        if (newValue == null)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
        else if (newValue.Value)
        {
            Enable();
        }
        else 
        {
            Disable();
        }
    }

    private void Enable() 
    {
        gameObject.SetActive(true);
    }
    private void Disable() 
    {
        gameObject.SetActive(false);
    }
}
