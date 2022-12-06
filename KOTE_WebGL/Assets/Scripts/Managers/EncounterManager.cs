using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EncounterManager : MonoBehaviour
{
    public GameObject EncounterContainer;
    public Button[] optionButtons;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(ShowEncounterPanel);
        GameManager.Instance.EVENT_POPULATE_ENCOUNTER_PANEL.AddListener(OnPopulateEncounter);
    }
    
    public void ShowEncounterPanel()
    {
        EncounterContainer.SetActive(true);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.EncounterData);
    }
    public void OnOptionOne()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
        GameManager.Instance.EVENT_ENCOUNTER_OPTION_SELECTED.Invoke(1);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }

    public void OnOptionTwo()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
        GameManager.Instance.EVENT_ENCOUNTER_OPTION_SELECTED.Invoke(2);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }

    private void OnPopulateEncounter(SWSM_EncounterData encounterData)
    {
        
    }

    private void PopulateEncounterText(string text)
    {
        
    }

    private void PopulateButtonText(List<string> optionText)
    {
        
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
