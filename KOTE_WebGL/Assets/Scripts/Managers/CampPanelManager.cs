using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CampPanelManager : MonoBehaviour
{
    public GameObject campContainer;
    public Button restButton;
    public Button smithButton;
    public TextMeshProUGUI skipButtonText;

    [SerializeField]
    private CardPairPanelManager upgradePanel;
    [SerializeField]
    private SelectCardsPanel cardPanel;
    private bool continueActivated;

    private void Start()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.AddListener(ShowCampPanel);
        GameManager.Instance.EVENT_CAMP_FINISH.AddListener(OnCampFinish);
        campContainer.SetActive(false);
    }

    private void ShowCampPanel()
    {
        campContainer.SetActive(true);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    public void OnRestSelected()
    {
        GameManager.Instance.EVENT_CAMP_HEAL.Invoke();
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Rest");
        OnCampFinish();
    }

    public void OnSmithSelected()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        OpenAndPopulatUpgradeCards();
    }

    private void DeactivateButtons()
    {
        restButton.interactable = false;
        smithButton.interactable = false;
    }

    private void SwitchToContinueButton()
    {
        skipButtonText.text = "Continue";
    }

    public void OnSkipSelected()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
        campContainer.SetActive(false);
    }

    private void ShowUpgradePanel(List<Card> cards)
    {
        SelectPanelOptions selectOptions = new SelectPanelOptions
        {
            HideBackButton = false,
            MustSelectAllCards = true,
            NumberOfCardsToSelect = 1,
            FireSelectWhenCardClicked = true
        };
        upgradePanel.HidePairPannel();

        cardPanel.ClearSelectList();
        cardPanel.PopulatePanel(cards, selectOptions, null, PopulatePairPanel);
    }

    private void PopulatePairPanel(string cardId) 
    {
        upgradePanel.ShowCardAndUpgrade(cardId, UpgradeCard, ClosePairPanel);
    }

    private async void UpgradeCard() 
    {
        CardUpgrade upgradeData = await FetchData.Instance.CampUpgradeCard(upgradePanel.OriginalCard.id);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Upgrade");
        upgradePanel.uiCardPair[0].gameObject.transform.DOScale(Vector3.zero, 1)
        .OnComplete(() => CloseUpgradePanels());
    }

    private async void OpenAndPopulatUpgradeCards()
    {
        List<Card> cards = await FetchData.Instance.GetUpgradeableCards();
        ShowUpgradePanel(cards);
    }

    private void CloseUpgradePanels() 
    {
        cardPanel.HidePanel();
        ClosePairPanel();
    }

    private void ClosePairPanel() 
    {
        upgradePanel.HidePairPannel();
    }

    private void OnCampFinish()
    {
        DeactivateButtons();
        CloseUpgradePanels();
        SwitchToContinueButton();
    }
}