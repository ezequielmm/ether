using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUpgradePanelManager : MonoBehaviour
{
    public GameObject upgradePanel;
    public GameObject uiCard;
    public GameObject upgradePanelLayout;
    public GameObject cardPairPanel;
    public UICardPrefabManager[] uiCardPair;

    private void Start()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_UPGRADE_PAIR.AddListener(OnShowUpgradePair);
        cardPairPanel.SetActive(false);
    }
    
    public void HidePanel()
    {
        upgradePanel.SetActive(false);
    }

    public void ShowPanel(Deck deck)
    {
        PopulateUpgradePanel(deck);
        upgradePanel.SetActive(true);
    }
    
    
    private void PopulateUpgradePanel(Deck deck)
    {
        foreach (Card card in deck.cards)
        {
            GameObject createdCard = Instantiate(uiCard, upgradePanelLayout.transform);
            SelectableUiCardManager cardManager = createdCard.GetComponentInChildren<SelectableUiCardManager>();
            cardManager.Populate(card);
           cardManager.cardSelectorButton.onClick.AddListener(() =>
            {
                OnCardSelect(card.id);
            });
        }
    }
    
    public void OnCardSelect(string id)
    {
        GameManager.Instance.EVENT_CAMP_GET_UPGRADE_PAIR.Invoke(id);
    }

    private void OnShowUpgradePair(Deck deck)
    {
        uiCardPair[0].populate(deck.cards[0]);
        uiCardPair[1].populate(deck.cards[1]);
        cardPairPanel.SetActive(true);
    }

    public void OnPairBackButton()
    {
        cardPairPanel.SetActive(false);
    }

    public void OnPairUpgradeConfirm()
    {
        GameManager.Instance.EVENT_CAMP_UPGRADE_CARD.Invoke(uiCardPair[0].id);
    }
}
