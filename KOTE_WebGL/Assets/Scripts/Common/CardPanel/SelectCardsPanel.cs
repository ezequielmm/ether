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
    private int selectedCards;
    private List<string> selectedCardIds = new List<string>();

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
                if (isOn)
                {
                    selectedCards++;
                    selectedCardIds.Add(cardManager.GetId());
                    
                }
                else
                {
                    selectedCards--;
                    selectedCardIds.Remove(cardManager.GetId());
                }

                if (selectedCards == cardsToSelect) selectButton.interactable = true;
                else selectButton.interactable = false;
            });
        }

        cardsToSelect = numberToSelect;
        selectButton.onClick.AddListener(() => { onFinishedSelection(selectedCardIds); });
        commonCardsContainer.SetActive(true);
    }
}