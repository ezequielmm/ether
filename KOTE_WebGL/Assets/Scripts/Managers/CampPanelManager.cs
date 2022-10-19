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

    private bool continueActivated;

    private void Start()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.AddListener(ShowCampPanel);
        GameManager.Instance.EVENT_CAMP_SHOW_UPRGRADEABLE_CARDS.AddListener(OnShowCardUpgradePanel);
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
        OnCampFinish();
    }

    public void OnSmithSelected()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.UpgradableCards);
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
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
        campContainer.SetActive(false);
    }
    
    private void OnShowCardUpgradePanel(Deck deck)
    {
        GameManager.Instance.EVENT_SHOW_SELECT_CARD_PANEL.Invoke(deck.cards, 1, (idList) =>
        {
            GameManager.Instance.EVENT_CAMP_GET_UPGRADE_PAIR.Invoke(idList[0]);
            GameManager.Instance.EVENT_HIDE_COMMON_CARD_PANEL.Invoke();
        });
    }

    private void OnCampFinish()
    {
        DeactivateButtons();
        SwitchToContinueButton();
    }
}