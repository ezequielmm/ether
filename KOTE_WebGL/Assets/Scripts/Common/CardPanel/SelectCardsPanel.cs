using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectCardsPanel : CardPanelBase
{
    public GameObject selectCardPrefab;
    public Button selectButton;
    public Button backButton;
    private int cardsToSelect;
    [SerializeField]  private int selectedCards;
    [SerializeField]  private List<string> selectedCardIds = new List<string>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameManager.Instance.EVENT_SHOW_SELECT_CARD_PANEL.AddListener(OnShowSelectCardPanel);
        GameManager.Instance.EVENT_SHOW_DIRECT_SELECT_CARD_PANEL.AddListener(OnShowDirectSelectPanel);
    }

    private void OnShowSelectCardPanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection)
    {
        PopulatePanel(selectableCards, selectOptions, onFinishedSelection);
    }

    private void OnShowDirectSelectPanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<string> onFinishedSelection)
    {
        PopulatePanel(selectableCards, selectOptions, onSelect: onFinishedSelection);
    }


    private void PopulatePanel(List<Card> selectableCards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection = null, Action<string> onSelect = null)
    {
        selectedCardIds.Clear();
        DestroyCards();
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

        cardsToSelect = selectOptions.NumberOfCardsToSelect;
        selectButton.onClick.AddListener(() => { onFinishedSelection(selectedCardIds); });
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
            }
            else if (cardManager.isSelected)
            {
                selectedCards--;
                selectedCardIds.Remove(cardManager.GetId());
                cardManager.isSelected = false;
            }

            if (selectedCards == cardsToSelect || !selectOptions.MustSelectAllCards)
                selectButton.gameObject.SetActive(true);
            else selectButton.gameObject.SetActive(false);

            cardManager.DetermineToggleColor();
        });
    }

    private void SetCardToFireOnClick(SelectableUiCardManager cardManager, Action<string> selectAction)
    {
        cardManager.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {
            selectAction.Invoke(cardManager.GetId());
        });
    }
}

// class to package different settings for the select menu
public class SelectPanelOptions
{
    public bool MustSelectAllCards;
    public bool HideBackButton;
    public bool FireSelectWhenCardClicked;
    public int NumberOfCardsToSelect;
}