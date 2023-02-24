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
        EncounterData encounterData = await FetchData.Instance.SelectEncounterOption(option);
        Populate(encounterData);
    }

    public void Show() 
    {
        creatureImage.gameObject.SetActive(true);
        EncounterContainer.SetActive(true);
    }

    // clear the panel so it doesn't show anything while waiting to be populated
    private void ClearPanel()
    {
        creatureImage.gameObject.SetActive(false);
        leftText.text = "";
        rightText.text = "";
        foreach (TMP_Text buttonText in buttonTexts)
        {
            buttonText.text = "";
        }
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
