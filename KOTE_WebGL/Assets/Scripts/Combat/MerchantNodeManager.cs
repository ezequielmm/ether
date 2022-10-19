using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantNodeManager : MonoBehaviour
{
    public GameObject shopContainer;

    [Header("Shop Panels")] 
    public Transform cardPanel;
    public Transform potionPanel;
    public Transform trinketPanel;

    [Header("Item Prefabs")]
    public GameObject uiCardPrefab;
    public GameObject uiPotionPrefab;
    public GameObject uiTrinketPrefab;

    MerchantData merchantData;

    private List<MerchantItem<MerchantData.Merchant<Card>>> cardItems = new List<MerchantItem<MerchantData.Merchant<Card>>>();
    private List<MerchantItem<MerchantData.Merchant<PotionData>>> potionItems = new List<MerchantItem<MerchantData.Merchant<PotionData>>>();
    private List<MerchantItem<MerchantData.Merchant<Trinket>>> trinketItems = new List<MerchantItem<MerchantData.Merchant<Trinket>>>();

    private void Start()
    {
        GameManager.Instance.EVENT_POPULATE_MERCHANT_PANEL.AddListener(PopulateMerchantNode);
        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener(ToggleVisibility);
    }

    public void ToggleVisibility(bool toggle)
    {
        // Set UI Visiable
        shopContainer.SetActive(toggle);
        
        if (toggle)
        {
            // Get merchant data
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.MerchantData);
        }
    }

    private void PopulateMerchantNode(MerchantData data) 
    {
        merchantData = data;
        RenderData();

        // Populate cards
        PopulateCards();
    }

    private void PopulateCards() 
    {
        foreach (var card in merchantData.cards) 
        {
            CardMerchantItem cardItem = Instantiate(uiCardPrefab, cardPanel).GetComponent<CardMerchantItem>();
        }
    }

    public void ClearMerchandise() 
    {
        // Clear cards
        foreach (Transform child in cardPanel)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Clear potions
        foreach (Transform child in potionPanel)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Clear trinkets
        foreach (Transform child in trinketPanel)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void ResetCards() 
    {
        foreach (var i in cardItems) 
        {
            i.OnDeselect();
        }
        foreach (var i in potionItems)
        {
            i.OnDeselect();
        }
        foreach (var i in trinketItems)
        {
            i.OnDeselect();
        }
    }

    private void PrepareBuy(int cost, MerchantData.IMerchant item) 
    {
    
    }

    public void RenderData() 
    {
        ClearMerchandise();
    }


    public void CloseMerchantPanel()
    {
        ToggleVisibility(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}