using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject combatContainer;
    [SerializeField] private GameObject Hand;
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject EnemyManager;
    
    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener(OnToggleCombatElements);
        GameManager.Instance.EVENT_SHOW_PLAYER_CHARACTER.AddListener(OnShowPlayerCharacter);
    }

    private void OnToggleCombatElements(bool data)
    {
        combatContainer.SetActive(data);
        Hand.SetActive(data);
        Player.SetActive(data);
        EnemyManager.SetActive(data);
    }

    // we need a way of only turning on the player for non-combat nodes
    private void OnShowPlayerCharacter()
    {
        combatContainer.SetActive(true);
        Hand.SetActive(false);
        Player.SetActive(true);
        EnemyManager.SetActive(false);
    }
}