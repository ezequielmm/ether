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
    public CardUpgradePanelManager upgradePanel;

    private bool continueActivated;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_CAMP_PANEL.AddListener(ShowCampPanel);
        GameManager.Instance.EVENT_CAMP_SHOW_UPRGRADEABLE_CARDS.AddListener(OnShowCardUpgradePanel);
        upgradePanel.HidePanel();
        campContainer.SetActive(false);
    }

    private void ShowCampPanel()
    {
        campContainer.SetActive(true);
    }

    public void OnRestSelected()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CampRecoverHealth);
        DeactivateButtons();
        SwitchToContinueButton();
    }

    public void OnSmithSelected()
    {
        //TODO add smithing functionality
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.UpgradeableCards);
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

    private void OnHealthRestored()
    {
        DeactivateButtons();
        SwitchToContinueButton();
    }

    private void OnShowCardUpgradePanel(Deck deck)
    {
        upgradePanel.ShowPanel(deck);
    }

    private void OnCampFinish()
    {
        DeactivateButtons();
        SwitchToContinueButton();
    }
}