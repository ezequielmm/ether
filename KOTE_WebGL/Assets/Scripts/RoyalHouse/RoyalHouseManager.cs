using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoyalHouseManager : MonoBehaviour
{
    public GameObject royalHouseContainer, confirmationContainer;

    public GameObject armoryItemPrefab;
    public GameObject blessingItemPrefab;

    public GameObject armoryContent;
    public Scrollbar armoryScroll;
    public GameObject blessingContent;
    public Scrollbar masterAtArmsScroll;

    [Space(20)]
    public TMP_Text encumbranceText, confirmationText;

    public List<GameObject> armoryItems = new List<GameObject>();
    public List<GameObject> blessingItems = new List<GameObject>();

    public int currentEncumbrance, maxEncumbrance, currentFief;

    private void Start()
    {
        GameManager.Instance.EVENT_ROYALHOUSES_ACTIVATION_REQUEST.AddListener(ActivateInnerRoyalHousePanel);
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
    /// <param name="prefab">prefab to instantiate </param>
    /// <param name="father">gameObject where it will be instantiated</param>
    /// <param name="itemName"></param>
    /// <param name="itemDescription"></param>
    /// <param name="itemCost"></param>
    /// <typeparam name="T">type of the object we are creating (ArmoryItem or BlessingItem) </typeparam>
    /// <returns>returns the gameObject instantiated so we can take control of it</returns>
    public GameObject CreateItemContent<T>(GameObject prefab, GameObject father, string itemName, string itemDescription, int itemCost)
    {
        GameObject currentItem = Instantiate(prefab, father.transform);

        if (typeof(T) == typeof(ArmoryItem))
        {
            currentItem.GetComponent<ArmoryItem>().SetProperties(itemName, itemDescription, itemCost);
        }
        else if (typeof(T) == typeof(BlessingItem))
        {
            currentItem.GetComponent<BlessingItem>().SetProperties(itemName, itemDescription, itemCost);
        }

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

    // I made two functions because I am setting armory by dropdown value change
    // and I can't pass more than one parameter on inspector.
    public void SetArmoryItems()
    {
        currentEncumbrance = 0;
        encumbranceText.text = $"{currentEncumbrance}/{maxEncumbrance}";
        SetItems<ArmoryItem>(armoryItems, armoryItemPrefab, armoryContent);
    }

    public void SetBlessingItems()
    {
        SetItems<BlessingItem>(blessingItems, blessingItemPrefab, blessingContent);
    }

    private void SetItems<T>(List<GameObject> objectsList, GameObject prefab, GameObject father) where T : RoyalHouseItem
    {
        objectsList.Clear();
        DestroyChildren(father);

        int randomArmoryItemsAmount = Random.Range(5, 15);

        for (int i = 0; i < randomArmoryItemsAmount; i++)
        {
            armoryItems.Add(CreateItemContent<T>(prefab,
                father, "Random item",
                "Random description for item",
                Random.Range(1, 11)));
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

    public void OnArmoryUpArrow()
    {
        if(armoryScroll.value < 1) armoryScroll.value += 0.1f;
    }

    public void OnArmoryDownArrow()
    {
        if(armoryScroll.value > 0) armoryScroll.value -= 0.1f;
    }
    
    public void OnMaaUpArrow()
    {
        if(masterAtArmsScroll.value < 1) masterAtArmsScroll.value += 0.1f;
    }

    public void OnMaaDownArrow()
    {
        if(masterAtArmsScroll.value > 0) masterAtArmsScroll.value -= 0.1f;
    }
}