using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class BlessingItem : MonoBehaviour
{
    public string itemName, itemDescription;
    public int itemFief;
    public TMP_Text itemNameText, itemDescriptionText, itemFiefText;
    public GameObject border;

    public bool selected;

    public void SetProperties(string itemName, string itemDescription, int itemFief)
    {
        this.itemName = itemName;
        this.itemDescription = itemDescription;
        this.itemFief = itemFief;

        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
        itemFiefText.text = $"{itemFief.ToString()} $fief";
    }

    public void OnBlessingItemSelected()
    {
        selected = !selected;
        GameManager.Instance.EVENT_SELECTBLESSING_ITEM.Invoke(this, selected);
    }
}