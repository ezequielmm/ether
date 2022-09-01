using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public GameObject EncounterContainer;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(ShowEncounterPanel);
    }
    
    public void ShowEncounterPanel()
    {
        EncounterContainer.SetActive(true);
    }
    public void OnOptionOne()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }

    public void OnOptionTwo()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}
