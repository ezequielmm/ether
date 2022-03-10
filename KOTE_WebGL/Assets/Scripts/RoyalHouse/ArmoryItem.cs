using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ArmoryItem : RoyalHouseItem
{
    public string itemName, itemDescription;
    public int itemEncumbrance;
    public TMP_Text itemNameText, itemDescriptionText, itemEncumbranceText;
    public GameObject border;

    public bool selected;

    public void SetProperties(string itemName, string itemDescription, int itemEncumbrance)
    {
        this.itemName = itemName;
        this.itemDescription = itemDescription;
        this.itemEncumbrance = itemEncumbrance;

        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        itemEncumbranceText.text = $"Encumbrance: {itemEncumbrance.ToString()}";
    }

    public void OnArmoryItemSelected()
    {
        selected = !selected;
        GameManager.Instance.EVENT_SELECTARMORY_ITEM.Invoke(this, selected);
    }
}