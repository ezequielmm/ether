using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestRulesPanel : MonoBehaviour
{
    [SerializeField]
    GameObject Panel;

    private bool shownPanelPreviously = false;

    void Awake()
    {
        DisablePanel();
    }

    private void OnEnable()
    {
        if (!shownPanelPreviously) 
        {
            shownPanelPreviously = true;
            EnablePanel();
        }
    }

    public void EnablePanel() 
    {
        TogglePannel(true);
    }
    public void DisablePanel() 
    {
        TogglePannel(false);
    }
    public void TogglePannel(bool newState) 
    {
        Panel.SetActive(newState);
    }
}
