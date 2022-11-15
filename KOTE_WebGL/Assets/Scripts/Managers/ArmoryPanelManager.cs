using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoryPanelManager : MonoBehaviour
{
    public GameObject panelContainer;

    public void Start()
    {
        panelContainer.SetActive(false);
        GameManager.Instance.EVENT_ARMORYPANEL_ACTIVATION_REQUEST.AddListener(ActivateContainer);
    }

    public void ActivateContainer(bool show)
    {
        int enableRegistration = PlayerPrefs.GetInt("enable_armory");
        if (enableRegistration == 1)
        {
            panelContainer.SetActive(show);
            return;
        }
        panelContainer.SetActive(false);
    }
    
    public void OnConfirmSelection()
    {
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.Invoke(true);
        ActivateContainer(false);
    }

    public void OnBackButton()
    {
        ActivateContainer(false);
    }
}
