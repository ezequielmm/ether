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
    [SerializeField]
    private TMPro.TextMeshProUGUI buyButtonText;

    [Header("Services")]
    [SerializeField]
    private MerchantCardPanel serviceCardPanel;
    [SerializeField]
    private CardPairPanelManager cardPairPanel;

    [Header("Service Buttons")]
    [SerializeField]
    private Button upgradeButton;
    [SerializeField]
    private Button removeButton;

    private List<MerchantItem<MerchantData.Merchant<Card>>> cardItems = new List<MerchantItem<MerchantData.Merchant<Card>>>();
    private List<MerchantItem<MerchantData.Merchant<PotionData>>> potionItems = new List<MerchantItem<MerchantData.Merchant<PotionData>>>();
    private List<MerchantItem<MerchantData.Merchant<Trinket>>> trinketItems = new List<MerchantItem<MerchantData.Merchant<Trinket>>>();

    private MerchantData.IMerchant selectedItem;
    private UICardPrefabManager selectedCard = null;

    private int gold => merchantData.coins;

    private void Start()
    {
        GameManager.Instance.EVENT_POPULATE_MERCHANT_PANEL.AddListener(PopulateMerchantNode);
        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener(ToggleVisibility);
        GameManager.Instance.EVENT_MERCHANT_PURCHASE_SUCCESS.AddListener(OnPurchaseResponse);
        ClearMerchandise();
        serviceCardPanel.OnCardClick.AddListener(OnCardSelection);
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

    private void OnPurchaseResponse(bool success) 
    {
        if (success) 
        {
            // Run Effects
        }
        // Update the merch node
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.MerchantData);
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
        SetSpeachBubble(data.speechBubble);

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
        selectedCard = null;
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

        upgradeButton.interactable = merchantData.upgradeCost <= gold;
        removeButton.interactable = merchantData.destroyCost <= gold;

        selectedItem = null;
        buyButton.interactable = false;
        buyButtonText.text = "BUY";
        selectedCard = null;
        upgradeCard = false;
        serviceCardPanel.HideCards();
        cardPairPanel.HidePairPannel();
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
        // Item Purchase
        if (selectedItem != null)
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Confirm Purchase");
            GameManager.Instance.EVENT_MERCHANT_BUY.Invoke(selectedItem.Type.ToLower(), selectedItem.Id);
        }
        // Service Purchase
        else if (selectedCard != null) 
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Confirm Purchase");
            GameManager.Instance.EVENT_MERCHANT_BUY.Invoke(upgradeCard ? "upgrade" : "remove", selectedCard.id);
        }
    }

    bool upgradeCard = false;
    public void UpgradeCard() 
    {
        ResetItems();
        // Run upgrade card pannel
        ShowCardPanel(merchantData.upgradeableCards);
        buyButtonText.text = "UPGRADE";
        upgradeCard = true;
    }

    public void RemoveCard() 
    {
        ResetItems();
        // Run remove card pannel
        ShowCardPanel(merchantData.playerCards);
        buyButtonText.text = "REMOVE";
    }

    public void OnCardSelection(string cardId, UICardPrefabManager card) 
    {
        card.Select();
        buyButton.interactable = true;
        DeselectCard();
        selectedCard = card;
        if (upgradeCard)
        {
            totalText.text = $"{merchantData.upgradeCost}";
            ShowCardComparison(cardId);
        }
        else 
        {
            totalText.text = $"{merchantData.destroyCost}";
        }
    }

    public void ShowCardComparison(string cardId) 
    {
        Card originalCardData = merchantData.upgradeableCards.Find(x => x.id == cardId);
        Card upgradedCardData = merchantData.upgradedCards.Find(x => x.id == cardId);

        cardPairPanel.ShowUpgrade(originalCardData, upgradedCardData);
    }

    public void HideCardComparison() 
    {
        cardPairPanel.HidePairPannel();
        DeselectCard();
        totalText.text = "0";
        buyButton.interactable = false;
    }

    private void DeselectCard() 
    {
        if (selectedCard != null) 
        {
            selectedCard.Deselect();
        }
    }

    private void ShowCardPanel(List<Card> cards) 
    {
        serviceCardPanel.useBackgroundImage = true;
        serviceCardPanel.scaleOnHover = false;
        serviceCardPanel.ShowCards(cards);

    }

    public void CloseCardPanel() 
    {
        ResetItems();
    }

    public void SetSpeachBubble(string text) 
    {
        SpeachBubbleContainer.SetActive(!string.IsNullOrEmpty(text));
        SpeachBubbleText.text = text;
    }


    public void CloseMerchantPanel()
    {
        ToggleVisibility(false);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}