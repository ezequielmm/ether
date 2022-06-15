using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public GameObject EncounterContainer;

    private void Start()
    {
        EncounterContainer.SetActive(false);
    }
    
    public void OnEncounterEntered()
    {
        EncounterContainer.SetActive(true);
    }
    public void OnOptionOne()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
    }

    public void OnOptionTwo()
    {
        //TODO add logic to display options
        EncounterContainer.SetActive(false);
    }
}
