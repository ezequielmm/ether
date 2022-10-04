using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableUiCardManager : MonoBehaviour
{
    public Button cardSelectorButton;
    [SerializeField]private UICardPrefabManager uiCardManager;

    public void Populate(Card card)
    {
        uiCardManager.populate(card);
    }
}
