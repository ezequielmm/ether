using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ConfirmationPanelManager : MonoBehaviour
{
    [Tooltip("The container for the node selection panel")]
    public GameObject confirmationPanelContainer;
    [Tooltip("The title text for the confirmation panel")]
    public TMP_Text titleText;
    

    // store the action so we can delete it after the player makes a selection
    private Action currentOnClickAction;
    private void Start()
    {
        confirmationPanelContainer.SetActive(false);
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL.AddListener(ShowNodeSelectionRequest);

    }

    private void ShowNodeSelectionRequest(string displayText, Action onClickFunction)
    {
        titleText.text = displayText;
        // store and add the onClickFunction for the button to listen to.
        currentOnClickAction = onClickFunction;
       
        confirmationPanelContainer.SetActive(true);
    }

    public void OnCancel()
    {
        // deactivate the panel and get rid of the action that was sent
        confirmationPanelContainer.SetActive(false);   
    }

    public void OnConfirmation()
    {
        currentOnClickAction();
        confirmationPanelContainer.SetActive(false);
    }
}
