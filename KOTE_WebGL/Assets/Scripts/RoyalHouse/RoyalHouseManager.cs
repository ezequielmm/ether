using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoyalHouseManager : MonoBehaviour
{
    public GameObject royalHouseContainer, confirmationContainer;

    public GameObject armoryItemPrefab;
    public GameObject blessingItemPrefab;

    public GameObject armoryContent;
    public GameObject blessingContent;

    [Space(20)]
    public TMP_Text encumbranceText, confirmationText;

    public List<GameObject> armoryItems = new List<GameObject>();
    public List<GameObject> blessingItems = new List<GameObject>();

    public int currentEncumbrance, maxEncumbrance, currentFief;

    private void Start()
    {
        GameManager.Instance.EVENT_SELECTARMORY_ITEM.AddListener(SetCurrentlyEncumbrance);
        GameManager.Instance.EVENT_SELECTBLESSING_ITEM.AddListener(SetCurrentlyFief);

        maxEncumbrance = 10;
        currentEncumbrance = 0;
        encumbranceText.text = $"{currentEncumbrance}/{maxEncumbrance}";

        SetArmoryItems();
        SetBlessingItems();
    }

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
    public GameObject CreateArmoryContent(GameObject prefab, GameObject father, string itemName, string itemDescription, int itemCost)
    {
        GameObject currentItem = Instantiate(prefab, father.transform);
        currentItem.GetComponent<ArmoryItem>().SetProperties(itemName, itemDescription, itemCost);
        return currentItem;
    }

    public GameObject CreateBlessingContent(GameObject prefab, GameObject father, string itemName, string itemDescription, int itemCost)
    {
        GameObject currentItem = Instantiate(prefab, father.transform);
        currentItem.GetComponent<BlessingItem>().SetProperties(itemName, itemDescription, itemCost);
        return currentItem;
    }

    public void SetCurrentlyEncumbrance(ArmoryItem armoryItem, bool selected)
    {
        if (selected)
        {
            if (currentEncumbrance + armoryItem.itemEncumbrance <= 10)
            {
                currentEncumbrance += armoryItem.itemEncumbrance;
                armoryItem.border.SetActive(true);
            }
            else
            {
                Debug.Log($"Max encumbrance reached: {maxEncumbrance}");
                armoryItem.selected = false;
                armoryItem.border.SetActive(false);
            }
        }
        else
        {
            currentEncumbrance -= armoryItem.itemEncumbrance;
            armoryItem.border.SetActive(false);
        }

        encumbranceText.text = $"{currentEncumbrance}/{maxEncumbrance}";
    }

    public void SetCurrentlyFief(BlessingItem blessingItem, bool selected)
    {
        blessingItem.border.SetActive(selected);
        currentFief += selected ? blessingItem.itemFief : -blessingItem.itemFief;
        confirmationText.text = $" Spend {currentFief} $fief on selection?";
    }

    public void SetArmoryItems()
    {
        armoryItems.Clear();
        DestroyChildren(armoryContent);

        int randomArmoryItemsAmount = Random.Range(5, 15);

        for (int i = 0; i < randomArmoryItemsAmount; i++)
        {
            armoryItems.Add(CreateArmoryContent(armoryItemPrefab,
                armoryContent, "Random armory item",
                "Random armory description for item",
                Random.Range(1, 11)));
        }
    }

    public void SetBlessingItems()
    {
        blessingItems.Clear();
        DestroyChildren(blessingContent);

        int randomArmoryItemsAmount = Random.Range(5, 15);

        for (int i = 0; i < randomArmoryItemsAmount; i++)
        {
            blessingItems.Add(CreateBlessingContent(blessingItemPrefab,
                blessingContent, "Random blessing item",
                "Random blessing description for item",
                Random.Range(0, 36)));
        }
    }

    public void DestroyChildren(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Destroy(go.transform.GetChild(i).gameObject);
        }
    }

    public void ActivateInnerSelectionsConfirmPanel(bool activate)
    {
        confirmationContainer.SetActive(activate);
    }

    public void ActivateInnerRoyalHousePanel(bool activate)
    {
        royalHouseContainer.SetActive(activate);
    }
}