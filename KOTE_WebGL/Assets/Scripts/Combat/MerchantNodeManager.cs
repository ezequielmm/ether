using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Shop Keeper")]
    public GameObject SpeachBubbleContainer;
    public TMPro.TextMeshProUGUI SpeachBubbleText;

    public Image ShopKeepImage;
    public List<Sprite> ShopKeepSprites = new List<Sprite>();

    MerchantData merchantData;

    [Header("Checkout")]
    [SerializeField]
    private TMPro.TextMeshProUGUI totalText;
    [SerializeField]
    private Button buyButton;

    private List<MerchantItem<MerchantData.Merchant<Card>>> cardItems = new List<MerchantItem<MerchantData.Merchant<Card>>>();
    private List<MerchantItem<MerchantData.Merchant<PotionData>>> potionItems = new List<MerchantItem<MerchantData.Merchant<PotionData>>>();
    private List<MerchantItem<MerchantData.Merchant<Trinket>>> trinketItems = new List<MerchantItem<MerchantData.Merchant<Trinket>>>();

    private MerchantData.IMerchant selectedItem;

    private int gold => merchantData.coins;

    private void Start()
    {
        GameManager.Instance.EVENT_POPULATE_MERCHANT_PANEL.AddListener(PopulateMerchantNode);
        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener(ToggleVisibility);
        ClearMerchandise();
    }

    public void ToggleVisibility(bool toggle)
    {
        // Set UI Visiable
        shopContainer.SetActive(toggle);
        
        if (toggle)
        {
            ClearMerchandise();
            // Get merchant data
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.MerchantData);
        }
    }

    private void PopulateMerchantNode(MerchantData data) 
    {
        merchantData = data;
        ClearMerchandise();

        // Populate cards
        PopulateCards();
        // Populate Potions
        PopulatePotions();
        // Populate Trinkets
        PopulateTrinkets();

        // Populate Shopkeeper
        ShopKeepImage.sprite = ShopKeepSprites[(int)Mathf.Clamp(data.shopkeeper, 0, ShopKeepSprites.Count - 1)];
        // Populate Speach Bubble
        SetSpeachBubble(data.speech_bubble);

        ResetItems();
    }

    private void PopulateTrinkets()
    {
        foreach (var trinket in merchantData.trinkets)
        {
            TrinketMerchantItem trinketItem = Instantiate(uiTrinketPrefab, trinketPanel).GetComponent<TrinketMerchantItem>();
            trinketItem.Populate(trinket);
            trinketItems.Add(trinketItem);
            trinketItem.PrepareBuy.AddListener(PrepareBuy);
        }
    }

    private void PopulatePotions()
    {
        foreach (var potion in merchantData.potions)
        {
            PotionMerchantItem potionItem = Instantiate(uiPotionPrefab, potionPanel).GetComponent<PotionMerchantItem>();
            potionItem.Populate(potion);
            potionItems.Add(potionItem);
            potionItem.PrepareBuy.AddListener(PrepareBuy);
        }
    }

    private void PopulateCards() 
    {
        foreach (var card in merchantData.cards) 
        {
            CardMerchantItem cardItem = Instantiate(uiCardPrefab, cardPanel).GetComponent<CardMerchantItem>();
            cardItem.Populate(card);
            cardItems.Add(cardItem);
            cardItem.PrepareBuy.AddListener(PrepareBuy);
        }
    }

    /// <summary>
    /// Removes all cards, potions, and trinkets from the store
    /// </summary>
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
        cardItems.Clear();
        trinketItems.Clear();
        potionItems.Clear();
        SetSpeachBubble(string.Empty);

        selectedItem = null;
        buyButton.interactable = false;
    }

    private void ResetItems() 
    {
        foreach (var i in cardItems) 
        {
            i.OnDeselect();
            i.CheckAffordability(gold);
        }
        foreach (var i in potionItems)
        {
            i.OnDeselect();
            i.CheckAffordability(gold);
        }
        foreach (var i in trinketItems)
        {
            i.OnDeselect();
            i.CheckAffordability(gold);
        }
        totalText.text = "0";

        selectedItem = null;
        buyButton.interactable = false;
    }

    private void PrepareBuy(int cost, MerchantData.IMerchant item) 
    {
        ResetItems();
        // Set total to the cost of card
        totalText.text = $"{cost}";
        // Set given merch
        selectedItem = item;
        buyButton.interactable = true;
    }

    /// <summary>
    /// Run when buying an item.
    /// </summary>
    public void BuyItem() 
    {
        // If no item is selected, no purchase is made
        if (selectedItem == null) 
        {
            return;
        }
        // Else use selectedItem and send message to backend for a purchase
        // TODO
    }

    public void UpgradeCard() 
    {
        ResetItems();
        // Run upgrade card pannel
    }

    public void RemoveCard() 
    {
        ResetItems();
        // Run remove card pannel
    }

    public void SetSpeachBubble(string text) 
    {
        SpeachBubbleContainer.SetActive(!string.IsNullOrEmpty(text));
        SpeachBubbleText.text = text;
    }


    public void CloseMerchantPanel()
    {
        ToggleVisibility(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}