using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EncounterData;

public class EncounterManager : MonoBehaviour
{
    public GameObject EncounterContainer;
    public Button[] optionButtons;
    public TMP_Text titleText;
    public TMP_Text[] buttonTexts;
    public TMP_Text leftText;
    public TMP_Text rightText;
    public GameObject rightTextPanel;
    public Image creatureImage;

    [SerializeField]
    private SelectCardsPanel cardPanel;
    [SerializeField]
    private CardPairPanelManager cardPairPanel;

    private void Start()
    {
        EncounterContainer.SetActive(false);
        ClearPanel();
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(ShowAndPopulate);
    }

    public async void ShowAndPopulate()
    {
        EncounterData encounterData = await FetchData.Instance.GetEncounterData();
        Populate(encounterData);
        Show();
    }

    public async void SelectOptionAndPopulate(int option)
    {
        // TODO: Work with backend to update this.
        // We will want the selection to tell us what to do next.
        // Not update encounter data (As it dose now).
        EncounterData encounterData = await FetchData.Instance.SelectEncounterOption(option);
        ShowAndPopulate();
    }

    public void Show()
    {
        creatureImage.gameObject.SetActive(true);
        EncounterContainer.SetActive(true);
    }

    /* Called directly from Backend. Typically as a 2nd response for
     * a Card Upgrade/Delete option. We will want one response per request.
     * TODO: Work with Backend to refactor.
     */
    public void ShowCardSelectionPanel(EncounterSelectionData selectionData)
    {
        switch (selectionData.Type)
        {
            case "upgrade":
                ShowUpgradeCardsPanel();
                break;
            default:
                ShowDeleteCardsPanel(selectionData);
                break;
        }
    }

    private async void ShowUpgradeCardsPanel() 
    {
        List<Card> upgradeableCards = await FetchData.Instance.GetUpgradeableCards();
        ShowCardPanel(upgradeableCards, new SelectPanelOptions
        {
            MustSelectAllCards = false,
            HideBackButton = true,
            FireSelectWhenCardClicked = true,
            NumberOfCardsToSelect = 1,
            ShowCardInCenter = false,
            NoSelectButton = false
        }, null,
        (string cardId) =>
        {
            ShowCardComparison(cardId);
        });
    }
    private void ShowCardComparison(string cardId)
    {
        cardPairPanel.ShowCardAndUpgrade(cardId, UpgradeCard, ClosePairPanel);
    }
    private async void UpgradeCard() 
    {
        CardUpgrade upgradeData = await FetchData.Instance.CampUpgradeCard(cardPairPanel.OriginalCard.id);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Upgrade");
        cardPairPanel.uiCardPair[0].gameObject.transform.DOScale(Vector3.zero, 1)
            .OnComplete(() => {
                CloseChildPanels();
                ShowAndPopulate();
            });
    }
    private void ClosePairPanel()
    {
        cardPairPanel.HidePairPannel();
    }

    private void ShowDeleteCardsPanel(EncounterSelectionData selectionData)
    {
        SelectPanelOptions panelOptions = new SelectPanelOptions
        {
            FireSelectWhenCardClicked = false,
            HideBackButton = true,
            MustSelectAllCards = true,
            NumberOfCardsToSelect = selectionData.NumberOfCardsToSelect,
            ShowCardInCenter = false
        };
        ShowCardPanel(selectionData.Cards, panelOptions, 
        (List<string> selectedCards) =>
        {
            SendData.Instance.SendCardsSelected(selectedCards);
            CloseChildPanels();
            ShowAndPopulate();
        }, null);
    }

    private void ShowCardPanel(List<Card> cards, SelectPanelOptions selectOptions,
        Action<List<string>> onFinishedSelection = null, Action<string> onSelect = null)
    {
        cardPanel.useBackgroundImage = true;
        cardPanel.scaleOnHover = false;
        cardPanel.PopulatePanel(cards, selectOptions, onFinishedSelection, onSelect);
    }

    private void ClearPanel()
    {
        creatureImage.gameObject.SetActive(false);
        leftText.text = "";
        rightText.text = "";
        foreach (TMP_Text buttonText in buttonTexts)
        {
            buttonText.text = "";
        }
        CloseChildPanels();
    }

    private void CloseChildPanels() 
    {
        cardPanel.HidePanel();
        ClosePairPanel();
    }

    private void Populate(EncounterData encounterData)
    {
        creatureImage.sprite = SpriteAssetManager.Instance.GetEncounterCreature(encounterData.imageId);
        creatureImage.SetNativeSize();
        titleText.text = encounterData.encounterName;
        SetDialogueText(encounterData.displayText);
        SetButtons(encounterData.buttons);
    }

    private void SetDialogueText(string text)
    {
        if (text.Length <= GameSettings.ENCOUNTER_TEXT_BOX_CHARACTER_COUNT)
        {
            leftText.text = text;
            rightTextPanel.SetActive(false);
            return;
        }

        string[] textSplit = text.Split('.');
        string firstText = "";
        int index = 0;
        while (firstText.Length < GameSettings.ENCOUNTER_TEXT_BOX_CHARACTER_COUNT)
        {
            if (firstText.Length + textSplit[index].Length < GameSettings.ENCOUNTER_TEXT_BOX_CHARACTER_COUNT)
            {
                firstText += textSplit[index];
                firstText += ". ";

                index++;
            }

            if (index >= textSplit.Length || firstText.Length + textSplit[index].Length >= GameSettings.ENCOUNTER_TEXT_BOX_CHARACTER_COUNT)
            {
                break;
            }
        }

        leftText.text = firstText;
        if (index < textSplit.Length)
        {
            rightTextPanel.SetActive(true);
            rightText.text = string.Join(".", textSplit[index..]);
        }
    }

    private void SetButtons(List<ButtonData> ButtonsData)
    {
        if (ButtonsData == null || ButtonsData.Count == 0)
        {
            SetButtonsToContinue();
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i >= ButtonsData.Count)
            {
                buttonTexts[i].text = "";
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            int buttonNum = i;

            if (ButtonsData[i].enabled)
            {
                buttonTexts[i].text = FormatEnabledButtonText(ButtonsData[i].text);
            }
            else if (ButtonsData[i].enabled == false)
            {
                buttonTexts[i].text = FormatDisabledButtonText(ButtonsData[i].text);
            }

            // update the on click listener
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                SelectOptionAndPopulate(buttonNum);
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
                DisableButtons();
            });

            // activate the button
            optionButtons[i].interactable = ButtonsData[i].enabled;
            optionButtons[i].gameObject.SetActive(true);
        }
    }

    private void SetButtonsToContinue()
    {
        // if there's no text sent, show the continue option and hide the rest of the buttons
        optionButtons[0].onClick.RemoveAllListeners();
        optionButtons[0].onClick.AddListener(() => GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke());
        buttonTexts[0].text = "<color=#FAB919><size=100%>Click Here To Continue";
        optionButtons[0].interactable = true;
        optionButtons[0].gameObject.SetActive(true);

        for (int i = 1; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = false;
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Formats the button text for enabled buttons
    /// </summary>
    /// <param name="text"> the text to format </param>
    /// <returns>the formatted button text</returns>
    private string FormatEnabledButtonText(string text)
    {
        int charIndex;
        // add in the color changes for the different options
        // if there's a letter at the beginning
        if (text.Contains(':'))
        {
            text = text.Insert(0, "<color=#E1D5A4> <size=120%>");
            charIndex = text.IndexOf(':');
            text = text.Insert(charIndex + 1, "<color=#FAB919> <size=100%>");
        }
        // if there's not
        else
        {
            text = text.Insert(0, "<color=#FAB919> <size=100%>");
        }

        charIndex = text.IndexOf('[');
        if (charIndex != -1)
        {
            text = text.Insert(charIndex - 1, "<color=#E1D5A4>");
        }

        return text;
    }

    /// <summary>
    /// turn the text gray, but keep the same formatting as the enabled text
    /// </summary>
    /// <param name="text">the text to format</param>
    /// <returns>the formatted text</returns>
    private string FormatDisabledButtonText(string text)
    {
        string startingText;
        if (text.Contains(':'))
        {
            startingText = "<color=#999999> <size=120%>";
            int charIndex = text.IndexOf(':');
            text = text.Insert(charIndex + 1, "<size=100%>");
        }
        else startingText = "<color=#999999>";
        
        text = text.Insert(0, startingText);
        
        return text;
    }


    private void DisableButtons()
    {
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }
    }
}

[Serializable]
public class EncounterData
{
    public string encounterName;
    public string imageId;
    public string displayText;
    public List<ButtonData> buttons = new();
    [Serializable]
    public class ButtonData
    {
        public string text;
        public bool enabled;
    }
}


[Serializable]
public class EncounterSelectionData
{
    [JsonProperty("cards")]
    public List<Card> Cards = new();
    [JsonProperty("cardsToTake")]
    public int NumberOfCardsToSelect;
    [JsonProperty("kind")]
    public string Type;
}