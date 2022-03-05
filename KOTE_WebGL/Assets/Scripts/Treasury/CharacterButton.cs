using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterButton : MonoBehaviour
{
    private TreasuryManager treasuryManager;

    private void Awake()
    {
        treasuryManager = FindObjectOfType<TreasuryManager>();
    }

    public void OnCharacterButton()
    {
        if (treasuryManager != null)
        {
            treasuryManager.OnCharacterButton();
        }
    }
}