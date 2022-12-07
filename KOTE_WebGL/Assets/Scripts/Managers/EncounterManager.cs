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
        PopulateEncounterText(encounterData.data.data.displayText);
        PopulateButtonText(encounterData.data.data.buttonText);
    }

    private void PopulateEncounterText(string text)
    {
        string[] textSplit = text.Split();
        int midpoint = textSplit.Length / 2;
        leftText.text = text.Substring(0, text.Length / 2);
        rightText.text = text.Substring(text.Length / 2, text.Length / 2 );
    }

    private void PopulateButtonText(List<string> optionText)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int buttonNum = i;
            buttonTexts[i].text = optionText[i];
            optionButtons[i].onClick.AddListener(() =>
            {
                GameManager.Instance.EVENT_ENCOUNTER_OPTION_SELECTED.Invoke(buttonNum);
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            });
        }
    }

    private void DisableButtons()
    {
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }
    }


    private void EnableButtons()
    {
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }
    }
}