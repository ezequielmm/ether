using DG.Tweening;
using System;
using UnityEngine;

public class CardPairPanelManager : MonoBehaviour
{
    public GameObject cardPairPanel;
    public UICardPrefabManager[] uiCardPair;

    [SerializeField]
    private bool manual = false;
    private Action OnConfirm;
    private Action OnBack;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_UPGRADE_PAIR.AddListener(OnShowUpgradePair);
        if (!manual)
        {
            GameManager.Instance.EVENT_UPGRADE_CONFIRMED.AddListener(OnUpgradeConfirmed);
        }
        cardPairPanel.SetActive(false);
    }

    private void OnShowUpgradePair(Deck deck)
    {
        uiCardPair[0].populate(deck.cards[0]);
        uiCardPair[1].populate(deck.cards[1]);
        cardPairPanel.SetActive(true);
    }

    public void ShowCardAndUpgrade(Card card, Action onConfirm = null, Action onBack = null) 
    {
        OnConfirm = onConfirm;
        OnBack = onBack;
        // Get Upgraded Pair
        GameManager.Instance.EVENT_GET_UPGRADE_PAIR.Invoke(card.id); // Will come back to OnShowUpgradePair
    }

    public void HidePairPannel() 
    {
        cardPairPanel.SetActive(false);
    }

    public void OnConfirmButton() 
    {
        if (!manual) 
        {
            OnPairUpgradeConfirm();
        }
        OnConfirm?.Invoke();
        OnConfirm = null;
    }

    public void OnPairBackButton()
    {
        cardPairPanel.SetActive(false);
        OnBack?.Invoke();
        OnBack = null;
    }

    public void OnPairUpgradeConfirm()
    {
        GameManager.Instance.EVENT_USER_CONFIRMATION_UPGRADE_CARD.Invoke(uiCardPair[0].id);
    }

    public void OnUpgradeConfirmed(SWSM_ConfirmUpgrade upgradeData)
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Upgrade");
        uiCardPair[0].gameObject.transform.DOScale(Vector3.zero, 1)
            .OnComplete(() =>
            {
                cardPairPanel.SetActive(false);
                GameManager.Instance.EVENT_HIDE_COMMON_CARD_PANEL.Invoke();
            });
    }
}
