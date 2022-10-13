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
    private int cardsToSelect;
    [SerializeField] [ReadOnly] private int selectedCards;
    [SerializeField] [ReadOnly] private List<string> selectedCardIds = new List<string>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameManager.Instance.EVENT_SHOW_SELECT_CARD_PANEL.AddListener(OnShowSelectCardPanel);
    }

    private void OnShowSelectCardPanel(List<Card> selectableCards, int numberToSelect,
        Action<List<string>> onFinishedSelection)
    {
        selectedCardIds.Clear();
        DestroyCards();
        foreach (Card card in selectableCards)
        {
            GameObject newCard = Instantiate(selectCardPrefab, gridCardsContainer.transform);
            SelectableUiCardManager cardManager = newCard.GetComponent<SelectableUiCardManager>();
            cardManager.Populate(card);

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

                if (selectedCards == cardsToSelect) selectButton.gameObject.SetActive(true);
                else selectButton.gameObject.SetActive(false);

                cardManager.DetermineToggleColor();
            });
        }

        cardsToSelect = numberToSelect;
        selectButton.onClick.AddListener(() => { onFinishedSelection(selectedCardIds); });
        selectButton.gameObject.SetActive(false);
        commonCardsContainer.SetActive(true);
    }
}