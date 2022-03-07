using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class is to have hard referenced the
/// text components when instantiating items (armory or blessing)
/// </summary>
public class ItemProperties : MonoBehaviour
{
    public TMP_Text itemName, itemDescription, itemCost;

    public void SetProperties(string itemName, string itemDescription, int itemCost)
    {
        this.itemName.text = itemName;
        this.itemDescription.text = itemDescription;
        this.itemCost.text = itemCost.ToString();
    }
}