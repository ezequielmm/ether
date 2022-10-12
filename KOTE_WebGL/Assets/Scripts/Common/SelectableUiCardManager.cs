using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableUiCardManager : MonoBehaviour
{
    public Toggle cardSelectorToggle;
    [SerializeField]private UICardPrefabManager uiCardManager;

    public void Populate(Card card)
    {
        uiCardManager.populate(card);
    }

    public string GetId()
    {
        return uiCardManager.id;
    }
}
