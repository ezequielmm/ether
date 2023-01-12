using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectCardsPanel : CardPanelBase
{
    public GameObject selectCardPrefab;
    public GameObject cardPlaceholder;
    public Button selectButton;
    public TMP_Text selectButtonText;
    public Button backButton;
    public Button hideCardOverlay;
    private int cardsToSelect;
    private int totalCardsSelected;
    [SerializeField] private int selectedCards;
    [SerializeField] private List<string> selectedCardIds = new List<string>();

    private SelectPanelOptions currentSettings;

    // we have to store this one as the button can be switched out to cancel instead
    private Action<List<String>> onFinishedSelection;
    private GameObject placeholderObject;

    // so we can move the card in the correct local space
    public RectTransform cardAreaTransform;
    private (SelectableUiCardManager cardManager, GameObject cardObject) currentSelectedCard;

    // variables to control showing the card in the center
    private int selectedCardGridIndex;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameManager.Instance.EVENT_SHOW_SELECT_CARD_PANEL.AddListener(OnShowSelectCardPanel);
        GameManager.Instance.EVENT_SHOW_DIRECT_SELECT_CARD_PANEL.AddListener(OnShowDirectSelectPanel);
        hideCardOverlay.gameObject.SetActive(false);
    }

    private void OnShowSelectCardPanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection)
    {
        ClearSelectList();
        totalCardsSelected = 0;
        PopulatePanel(selectableCards, selectOptions, onFinishedSelection);
    }

    private void OnShowDirectSelectPanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<string> onFinishedSelection)
    {
        ClearSelectList();
        totalCardsSelected = 0;
        PopulatePanel(selectableCards, selectOptions, onSelect: onFinishedSelection);
    }


    private void PopulatePanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection = null, Action<string> onSelect = null)
    {
        selectedCardIds.Clear();
        DestroyCards();
        currentSettings = selectOptions;
        foreach (Card card in selectableCards)
        {
            GameObject newCard = Instantiate(selectCardPrefab, gridCardsContainer.transform);
            SelectableUiCardManager cardManager = newCard.GetComponent<SelectableUiCardManager>();
            cardManager.Populate(card);

            if (selectOptions.FireSelectWhenCardClicked)
            {
                SetCardToFireOnClick(cardManager, onSelect);
                continue;
            }

            SetCardToBeToggled(cardManager, selectOptions);
        }

        placeholderObject = Instantiate(cardPlaceholder, gridCardsContainer.transform);
        placeholderObject.SetActive(false);

        cardsToSelect = selectOptions.NumberOfCardsToSelect;
        this.onFinishedSelection = onFinishedSelection;
        UpdateSelectButton();
        selectButton.gameObject.SetActive(!selectOptions.MustSelectAllCards);
        backButton.gameObject.SetActive(!selectOptions.HideBackButton);
        commonCardsContainer.SetActive(true);
    }


    private void SetCardToBeToggled(SelectableUiCardManager cardManager, SelectPanelOptions selectOptions)
    {
        // set the toggle behavior to keep track what cards are selected
        cardManager.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {
            if (cardManager.isSelected == false && selectedCards != cardsToSelect)
            {
                selectedCards++;
                selectedCardIds.Add(cardManager.GetId());
                cardManager.isSelected = true;
                if (selectOptions.ShowCardInCenter)
                {
                    ShowCardInCenter(cardManager.gameObject, cardManager);
                }
            }
            else if (cardManager.isSelected)
            {
                if (selectOptions.ShowCardInCenter)
                {
                    ReturnCardToGrid(cardManager);
                }
                else
                {
                    selectedCards--;
                    selectedCardIds.Remove(cardManager.GetId());
                    cardManager.isSelected = false;
                }
            }

            if (selectedCards == cardsToSelect || !selectOptions.MustSelectAllCards)
                selectButton.gameObject.SetActive(true);
            else selectButton.gameObject.SetActive(false);

            UpdateSelectButton();
            cardManager.DetermineToggleColor();
        });
    }

    private void ShowCardInCenter(GameObject selectedCard, SelectableUiCardManager cardManager)
    {
        if (currentSelectedCard.cardManager != null && currentSelectedCard.cardObject != null) return;
        List<SelectableUiCardManager> cardManagerList =
            gridCardsContainer.GetComponentsInChildren<SelectableUiCardManager>().ToList();
        selectedCardGridIndex = cardManagerList.FindIndex(card => card.GetId() == cardManager.GetId());

        selectedCard.transform.SetParent(cardAreaTransform);
        placeholderObject.SetActive(true);
        placeholderObject.transform.SetSiblingIndex(selectedCardGridIndex);
        selectedCard.transform.SetAsLastSibling();
        selectedCard.transform.DOLocalMove(
            new Vector3(cardAreaTransform.rect.width / 2, -cardAreaTransform.rect.height / 2), 1);
        currentSelectedCard = (cardManager, selectedCard);
        hideCardOverlay.gameObject.SetActive(true);
    }

    // public override for the hide overlay
    public void ReturnCardToGrid()
    {
        ReturnCardToGrid(null);
    }

    private void ReturnCardToGrid(SelectableUiCardManager cardManager)
    {
        if (cardManager != null && cardManager.GetId() != currentSelectedCard.cardManager.GetId()) return;
        RectTransform cardTransform = currentSelectedCard.cardObject.transform as RectTransform;
        RectTransform placeholderTransform = placeholderObject.transform as RectTransform;
        cardTransform.DOAnchorPos(placeholderTransform.anchoredPosition, 1).OnComplete(() =>
        {
            // add the card back to the grid
            currentSelectedCard.cardObject.transform.SetParent(gridCardsContainer.transform);
            placeholderObject.SetActive(false);
            currentSelectedCard.cardObject.transform.SetSiblingIndex(selectedCardGridIndex);
            // update the selected card
            selectedCards--;
            selectedCardIds.Remove(currentSelectedCard.cardManager.GetId());
            currentSelectedCard.cardManager.isSelected = false;
            // and clear the active selected card
            currentSelectedCard.cardManager = null;
            currentSelectedCard.cardObject = null;
            UpdateSelectButton();
            hideCardOverlay.gameObject.SetActive(false);
        });
    }

    private void SetCardToFireOnClick(SelectableUiCardManager cardManager, Action<string> selectAction)
    {
        cardManager.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            selectAction.Invoke(cardManager.GetId());
        });
    }

    public void HidePanel()
    {
        ClearSelectList();
        HideCardSelectPanel();
    }

    private void ClearSelectList()
    {
        selectedCards = 0;
        selectedCardIds.Clear();
    }

    private void UpdateSelectButton()
    {
        if (currentSettings.HideBackButton)
        {
            if (selectedCardIds.Count > 0)
            {
                selectButtonText.text = "Select";
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() =>
                {
                    onFinishedSelection(selectedCardIds);
                    totalCardsSelected += selectedCards;
                    ClearSelectList();
                    DeleteSelectedCard();
                    UpdateSelectButton();
                    if (totalCardsSelected == cardsToSelect)
                    {
                        HidePanel();
                    }
                    else if (totalCardsSelected > cardsToSelect)
                    {
                        Debug.LogError("Too many cards selected!");
                    }
                });
            }
            else if (selectedCardIds.Count == 0)
            {
                selectButtonText.text = "Cancel";
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => { HidePanel(); });
            }
        }
    }

    private void DeleteSelectedCard()
    {
        Destroy(currentSelectedCard.cardObject);
        placeholderObject.SetActive(false);
        currentSelectedCard.cardManager = null;
        currentSelectedCard.cardObject = null;
    }
}

// class to package different settings for the select menu
public class SelectPanelOptions
{
    public bool MustSelectAllCards;
    public bool HideBackButton;
    public bool FireSelectWhenCardClicked;
    public int NumberOfCardsToSelect;
    public bool ShowCardInCenter;
}