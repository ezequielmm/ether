using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterManager : MonoBehaviour
{
    public GameObject EncounterContainer;
    public Button[] optionButtons;
    public TMP_Text[] buttonTexts;
    public TMP_Text leftText;
    public TMP_Text rightText;
    public Image creatureImage;

    private void Start()
    {
        EncounterContainer.SetActive(false);
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(ShowEncounterPanel);
        GameManager.Instance.EVENT_POPULATE_ENCOUNTER_PANEL.AddListener(OnPopulateEncounter);
    }

    public void ShowEncounterPanel()
    {
        EncounterContainer.SetActive(true);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.EncounterData);
    }

    private void OnPopulateEncounter(SWSM_EncounterData encounterData)
    {
        creatureImage.sprite = SpriteAssetManager.Instance.GetEncounterCreature(encounterData.data.data.imageId);
        creatureImage.SetNativeSize();
        PopulateEncounterText(encounterData.data.data.displayText);
        PopulateButtonText(encounterData.data.data.buttons);
    }

    private void PopulateEncounterText(string text)
    {
        if (text.Length <= 200)
        {
            leftText.text = text;
            rightText.gameObject.SetActive(false);
            return;
        }

        string[] textSplit = text.Split('.');
        string firstText = "";
        int index = 0;
        while (firstText.Length < 200)
        {
            if (firstText.Length + textSplit[index].Length < 200)
            {
                firstText += textSplit[index];
                firstText += ". ";

                index++;
            }

            if (index >= textSplit.Length || firstText.Length + textSplit[index].Length >= 200)
            {
                break;
            }
        }

        leftText.text = firstText;
        if (index < textSplit.Length)
        {
            rightText.gameObject.SetActive(true);
            rightText.text = string.Join(".", textSplit[index..]);
        }
    }

    private void PopulateButtonText(List<ButtonData> ButtonsData)
    {
        if (ButtonsData == null || ButtonsData.Count == 0)
        {
            SetButtonsToContinue();
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i > ButtonsData.Count)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            int buttonNum = i;
            buttonTexts[i].text = FormatButtonText( ButtonsData[i].text);
            
            // update the on click listener
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                GameManager.Instance.EVENT_ENCOUNTER_OPTION_SELECTED.Invoke(buttonNum);
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
        buttonTexts[0].text = "<color=#E1D5A4> <size=120%>A: <color=#FAB919><size=100%>Continue";
        optionButtons[0].interactable = true;
        optionButtons[0].gameObject.SetActive(true);

        for (int i = 1; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = false;
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    private string FormatButtonText(string text)
    {
        // add in the color changes for the different options
        text = text.Insert(0, "<color=#E1D5A4> <size=120%>");
        int charIndex = text.IndexOf(':');
        text = text.Insert(charIndex + 1, "<color=#FAB919> <size=100%>");
        charIndex = text.IndexOf('[');
        text = text.Insert(charIndex - 1, "<color=#E1D5A4>");

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