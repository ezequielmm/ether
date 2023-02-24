using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    MerchantData merchant;

    [Header("Checkout")]
    [SerializeField]
    private TMPro.TextMeshProUGUI totalText;
    [SerializeField]
    private Button buyButton;
    [SerializeField]
    private TMPro.TextMeshProUGUI buyButtonText;

    [Header("Services")]
    [SerializeField]
    private SelectCardsPanel serviceCardPanel;
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
    private string selectedCard = string.Empty;

    private int gold => merchant.coins;

    private void Start()
    {
        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener(ToggleVisibility);
        GameManager.Instance.EVENT_MERCHANT_PURCHASE_SUCCESS.AddListener(OnPurchaseResponse);
        serviceCardPanel.OnBackButton.AddListener(ResetItems);
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
            UpdateMerchantData();
        }
    }

    private void OnPurchaseResponse(bool success) 
    {
        if (success) 
        {
            // Run Effects
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Confirm Purchase");
        }
        UpdateMerchantData();
    }

    private async void UpdateMerchantData() 
    {
        merchant = await FetchData.Instance.GetMerchantData();
        RenderMerchant();
    }

    private void RenderMerchant() 
    {
        ClearMerchandise();

        PopulateCards();
        PopulatePotions();
        PopulateTrinkets();
        PopulateShopKeep();

        ResetItems();
    }

    private void PopulateShopKeep() 
    {
        ShopKeepImage.sprite = ShopKeepSprites[(int)Mathf.Clamp(merchant.shopkeeper, 0, ShopKeepSprites.Count - 1)];
        SetSpeachBubble(merchant.speechBubble);
    }

    private void PopulateTrinkets()
    {
        foreach (var trinket in merchant.trinkets)
        {
            TrinketMerchantItem trinketItem = Instantiate(uiTrinketPrefab, trinketPanel).GetComponent<TrinketMerchantItem>();
            trinketItem.Populate(trinket);
            trinketItems.Add(trinketItem);
            trinketItem.PrepareBuy.AddListener(PrepareBuy);
        }
    }

    private void PopulatePotions()
    {
        foreach (var potion in merchant.potions)
        {
            PotionMerchantItem potionItem = Instantiate(uiPotionPrefab, potionPanel).GetComponent<PotionMerchantItem>();
            potionItem.Populate(potion);
            potionItems.Add(potionItem);
            potionItem.PrepareBuy.AddListener(PrepareBuy);
        }
    }

    private void PopulateCards() 
    {
        foreach (var card in merchant.cards) 
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
        selectedCard = string.Empty;
        serviceCardPanel.HidePanel();
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

        upgradeButton.interactable = merchant.upgradeCost <= gold;
        removeButton.interactable = merchant.destroyCost <= gold;

        selectedItem = null;
        buyButton.interactable = false;
        buyButtonText.text = "BUY";
        selectedCard = string.Empty;
        upgradeCard = false;
        serviceCardPanel.ClearSelectList();
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
            GameManager.Instance.EVENT_MERCHANT_BUY.Invoke(selectedItem.Type.ToLower(), selectedItem.Id);
        }
        // Service Purchase
        else if (!string.IsNullOrWhiteSpace(selectedCard)) 
        {
            GameManager.Instance.EVENT_MERCHANT_BUY.Invoke(upgradeCard ? "upgrade" : "remove", selectedCard);
        }
    }

    bool upgradeCard = false;
    public async void UpgradeCard() 
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ResetItems();
        // Run upgrade card pannel
        List<Card> upgradeableCards = await FetchData.Instance.GetUpgradeableCards();
        ShowCardPanel(upgradeableCards, new SelectPanelOptions 
        {
            MustSelectAllCards = false,
            HideBackButton = false,
            FireSelectWhenCardClicked = true,
            NumberOfCardsToSelect = 1,
            ShowCardInCenter = false,
            NoSelectButton = false
        }, null,
        (string selected) => 
        {
            upgradeCard = true;
            selectedCard = selected;
            OnCardSelection(selectedCard);
        });
        buyButtonText.text = "UPGRADE";
    }

    public void RemoveCard() 
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        ResetItems();
        // Run remove card pannel
        ShowCardPanel(merchant.removeableCards, new SelectPanelOptions
        {
            MustSelectAllCards = true,
            HideBackButton = false,
            FireSelectWhenCardClicked = true,
            NumberOfCardsToSelect = 1,
            ShowCardInCenter = false,
            NoSelectButton = true
        },null,
        (string selected) =>
        {
            upgradeCard = false;
            selectedCard = selected;
            OnCardSelection(selectedCard);
        });
        buyButtonText.text = "REMOVE";
    }

    public void OnCardSelection(string cardId) 
    {
        buyButton.interactable = true;
        if (upgradeCard)
        {
            totalText.text = $"{merchant.upgradeCost}";
            ShowCardComparison(cardId);
        }
        else 
        {
            totalText.text = $"{merchant.destroyCost}";
        }
    }

    public void ShowCardComparison(string cardId) 
    {
        cardPairPanel.ShowCardAndUpgrade(cardId, () => 
        {
            // On Confirm
            BuyItem();
            cardPairPanel.HidePairPannel();
        }, () => 
        {
            // On Back
            ResetItems();
            cardPairPanel.HidePairPannel();
        });
    }

    private void ShowCardPanel(List<Card> cards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection = null, Action<string> onSelect = null) 
    {
        serviceCardPanel.useBackgroundImage = true;
        serviceCardPanel.scaleOnHover = false;
        serviceCardPanel.PopulatePanel(cards, selectOptions, onFinishedSelection, onSelect);
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

[Serializable]
public class MerchantData
{
    public int coins;
    public int shopkeeper;
    public string speechBubble;
    [JsonProperty("playerCards")]
    public List<Card> removeableCards { get; } = new List<Card>();
    public int upgradeCost;
    public int destroyCost;
    public List<Merchant<Card>> cards = new List<Merchant<Card>>();
    public List<Merchant<Card>> neutralCards = new List<Merchant<Card>>();// TODO
    public List<Merchant<Trinket>> trinkets = new List<Merchant<Trinket>>();
    public List<Merchant<PotionData>> potions = new List<Merchant<PotionData>>();

    [Serializable]
    public class Merchant<T> : IMerchant
    {
        [JsonProperty("itemId")]
        public int ItemId { get; set; }
        [JsonProperty("cost")]
        public int Coin { get; set; }
        [JsonProperty("isSold")]
        public bool IsSold { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("item")]
        public T Item;
    }

    public interface IMerchant
    {
        public int ItemId { get; set; }
        public int Coin { get; set; }
        public bool IsSold { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
    }
}