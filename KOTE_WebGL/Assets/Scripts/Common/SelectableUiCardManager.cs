using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableUiCardManager : MonoBehaviour
{
    public Toggle cardSelectorToggle;
    // keep track of this manually so we can have multiple cards selected 
    public bool isSelected;
    [SerializeField]private UICardPrefabManager uiCardManager;

    public void Populate(Card card)
    {
        uiCardManager.populate(card);
    }

    public string GetId()
    {
        return uiCardManager.id;
    }

    public void DetermineToggleColor()
    {
        if(isSelected) cardSelectorToggle.targetGraphic.color = Color.green;
        if(!isSelected) cardSelectorToggle.targetGraphic.color = Color.clear;
    }
}
