using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnvironmentManager : MonoBehaviour
{
    public Image combatBg;

    void Start()
    {
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.AddListener(OnMapNodeSelected);
    }

    private void OnMapNodeSelected(int act, int step)
    {
            // else set that as the background and continue
            combatBg.sprite = SpriteAssetManager.Instance.GetCombatBackground(act, step);
    }
}