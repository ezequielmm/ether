using DG.Tweening;
using System;
using UnityEngine;

public class CardPairPanelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPairPanel;
    public UICardPrefabManager[] uiCardPair;

    public Card OriginalCard { get; private set; }
    public Card NewCard { get; private set; }

    private Action OnConfirm;
    private Action OnBack;

    private void Start()
    {
        cardPairPanel.SetActive(false);
    }

    private void OnShowUpgradePair(Deck deck)
    {
        OriginalCard = deck.cards[0];
        NewCard = deck.cards[1];

        uiCardPair[0].Populate(OriginalCard);
        uiCardPair[1].Populate(NewCard);
        cardPairPanel.SetActive(true);
    }

    public async void ShowCardAndUpgrade(string cardId, Action onConfirm = null, Action onBack = null) 
    {
        OnConfirm = onConfirm;
        OnBack = onBack;
        Deck cardPairs = await FetchData.Instance.GetCardUpgradePair(cardId);
        OnShowUpgradePair(cardPairs);
    }

    public void HidePairPannel() 
    {
        cardPairPanel.SetActive(false);
    }

    public void OnConfirmButton() 
    {
        OnConfirm?.Invoke();
        ClearActions();
    }

    public void OnPairBackButton()
    {
        HidePairPannel();
        OnBack?.Invoke();
        ClearActions();
    }

    private void ClearActions() 
    {
        OnBack = null;
        OnConfirm = null;
    }
}
