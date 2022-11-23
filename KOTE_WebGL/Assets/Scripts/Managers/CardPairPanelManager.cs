using DG.Tweening;
using UnityEngine;

public class CardPairPanelManager : MonoBehaviour
{
    public GameObject cardPairPanel;
    public UICardPrefabManager[] uiCardPair;

    private void Start()
    {
        GameManager.Instance.EVENT_UPGRADE_SHOW_UPGRADE_PAIR.AddListener(OnShowUpgradePair);
        GameManager.Instance.EVENT_UPGRADE_CONFIRMED.AddListener(OnUpgradeConfirmed);
        cardPairPanel.SetActive(false);
    }

    private void OnShowUpgradePair(Deck deck)
    {
        uiCardPair[0].populate(deck.cards[0]);
        uiCardPair[1].populate(deck.cards[1]);
        cardPairPanel.SetActive(true);
    }

    public void ShowUpgrade(Card card, Card upgradedCard) 
    {
        uiCardPair[0].populate(card);
        uiCardPair[1].populate(upgradedCard);

        cardPairPanel.SetActive(true);
    }

    public void HidePairPannel() 
    {
        cardPairPanel.SetActive(false);
    }

    public void OnPairBackButton()
    {
        cardPairPanel.SetActive(false);
    }

    public void OnPairUpgradeConfirm()
    {
        GameManager.Instance.EVENT_CAMP_UPGRADE_CARD.Invoke(uiCardPair[0].id);
    }

    public void OnUpgradeConfirmed(SWSM_ConfirmUpgrade upgradeData)
    {
        uiCardPair[0].gameObject.transform.DOScale(Vector3.zero, 1)
            .OnComplete(() =>
            {
                cardPairPanel.SetActive(false);
                GameManager.Instance.EVENT_HIDE_COMMON_CARD_PANEL.Invoke();
            });
    }
}
