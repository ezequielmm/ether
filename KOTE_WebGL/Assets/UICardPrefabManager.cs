using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UICardPrefabManager : MonoBehaviour
{
    public TextMeshProUGUI energyTF;
    public TextMeshProUGUI typeTF;
    public TextMeshProUGUI rarityTF;
    public TextMeshProUGUI descriptionTF;
    public string id;

    private Card card;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void populate(Card card)
    {
        energyTF.SetText(card.energy.ToString());
        typeTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
    }
}
