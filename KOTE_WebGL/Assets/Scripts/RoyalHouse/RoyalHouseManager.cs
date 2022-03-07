using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalHouseManager : MonoBehaviour
{
    [Serializable]
    public class ArmoryItem
    {
        public string name;
        public string description;
        public int encumbrance;
    }

    [Serializable]
    public class BlessingItem
    {
        public string name;
        public string description;
        public int fief;
    }

    public GameObject confirmationPanel;

    public GameObject armoryItemPrefab;
    public GameObject blessingItemPrefab;

    public GameObject armoryContent;
    public GameObject blessingContent;

    public List<GameObject> armoryItems = new List<GameObject>();
    public List<GameObject> blessingItems = new List<GameObject>();

    /// <summary>
    /// This function is to create an object in UI
    /// 
    /// we'll call this function for every item we get to instantiate in
    /// the UI (currently not using it since we are using placeholders)
    /// </summary>
    /// <param name="prefab"> prefab to instantiate (armory item or blessing item) </param>
    /// <param name="father"> gameObject where it will be instantiated (armory content or blessing content) </param>
    /// <param name="itemName"></param>
    /// <param name="itemDescription"></param>
    /// <param name="itemCost"></param>
    /// <returns> returns the gameObject instantiated so we can take control of it</returns>
    public GameObject CreateItemContent(GameObject prefab, GameObject father, string itemName, string itemDescription, int itemCost)
    {
        GameObject currentItem = Instantiate(prefab, father.transform);
        currentItem.GetComponent<ItemProperties>().SetProperties(itemName, itemDescription, itemCost);
        return currentItem;
    }

    public void ActivateInnerSelectionsConfirmPanel(bool activate)
    {
        confirmationPanel.SetActive(activate);
    }
}