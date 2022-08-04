using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private GameObject combatContainer;

    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener(OnToggleCombatElements);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener(OnMapPanelToggle);
    }

    private void OnToggleCombatElements(bool data)
    {
        combatContainer.SetActive(data);
    }

    private void OnMapPanelToggle(bool mapOpen) 
    {
        combatContainer.SetActive(!mapOpen);
    }
}