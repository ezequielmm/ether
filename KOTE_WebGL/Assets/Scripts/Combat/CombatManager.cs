using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject combatContainer;
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemyManager;
    [SerializeField] private GameObject pointer;
    
    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener(OnToggleCombatElements);
        GameManager.Instance.EVENT_SHOW_PLAYER_CHARACTER.AddListener(OnShowPlayerCharacter);
    }

    private void OnToggleCombatElements(bool data)
    {
        combatContainer.SetActive(data);
        hand.SetActive(data);
        player.SetActive(data);
        enemyManager.SetActive(data);
        pointer.SetActive(data);
    }

    // we need a way of only turning on the player for non-combat nodes
    private void OnShowPlayerCharacter()
    {
        combatContainer.SetActive(true);
        hand.SetActive(false);
        player.SetActive(true);
        enemyManager.SetActive(false);
        pointer.SetActive(false);
    }
}