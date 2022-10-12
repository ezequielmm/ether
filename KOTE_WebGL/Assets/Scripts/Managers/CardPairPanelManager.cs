using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPairPanelManager : MonoBehaviour
{
  
    public GameObject cardPairPanel;
    public UICardPrefabManager[] uiCardPair;

    private void Start()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_UPGRADE_PAIR.AddListener(OnShowUpgradePair);
        cardPairPanel.SetActive(false);
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
