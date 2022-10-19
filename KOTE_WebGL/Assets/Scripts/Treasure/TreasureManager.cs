using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureManager : MonoBehaviour
{
    [SerializeField]
    private GameObject treasureContainer;

    void Start()
    {
        treasureContainer.SetActive(false);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener(ContainerToggle);
    }

    private void ContainerToggle(bool value) 
    {
        treasureContainer.SetActive(value);
        if (value == true)
        {
            GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        }
    }

    private void StartCombat() 
    {
        // Enable combat UI
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(true);
        // Disable treasure container
        ContainerToggle(false);
    }

    public void OnTreasureDone()
    {
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}
