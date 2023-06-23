using System;
using TMPro;
using UnityEngine;

public class ConfirmationPanelManager : MonoBehaviour
{
    [Tooltip("The container for the node selection panel")]
    public GameObject confirmationPanelContainer;
    [Tooltip("The title text for the confirmation panel")]
    public TMP_Text titleText;
    [Tooltip("The text for the confirmation button")]
    public TMP_Text confirmText;
    [Tooltip("The text for the cancel button")]
    public TMP_Text cancelText;
    [Tooltip("The confirm button gameobject")]
    public GameObject confirmButton;
    [Tooltip("The cancel button gameobject")]
    public GameObject cancelButton;

    // store the action so we can delete it after the player makes a selection
    private Action currentOnConfirmAction;
    private Action currentOnCancelFunction;
    private void Start()
    {
        confirmationPanelContainer.SetActive(false);
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_BACK_ACTION.AddListener(ShowConfirmationPanelWithBackAction);
        GameManager.Instance.EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL.AddListener(ShowConfirmationPanelWIthFullControl);

    }

    public void ShowBackButton(bool showBack)
    {
        cancelButton.SetActive(showBack);
    }
    
    public void ShowConfirmationPanel(string displayText, Action onConfirmFunction)
    {
        if (confirmationPanelContainer.activeSelf) return;
        confirmButton.SetActive(true);
        cancelButton.SetActive(true);

        titleText.text = displayText;
        // store and add the onClickFunction for the button to listen to.
        currentOnConfirmAction = onConfirmFunction;
        confirmText.text = "Confirm";
        cancelText.text = "Back";
       
        confirmationPanelContainer.SetActive(true);
    }

    //override to allow the back button to do more than hide the confirmation window
    private void ShowConfirmationPanelWithBackAction(string displayText, Action onConfirmFunction, Action onCancelFunction)
    {
        if (confirmationPanelContainer.activeSelf) return;
        confirmButton.SetActive(true);
        cancelButton.SetActive(true);

        titleText.text = displayText;
        // store and add the onClickFunction for the button to listen to.
        currentOnConfirmAction = onConfirmFunction;
        currentOnCancelFunction = onCancelFunction;
        confirmText.text = "Confirm";
        cancelText.text = "Back";
        
        confirmationPanelContainer.SetActive(true);
    }

    private void ShowConfirmationPanelWIthFullControl(string displayText, Action onConfirmFunction,
        Action onCancelFunction, string[] buttonTexts)
    {
        if (confirmationPanelContainer.activeSelf) return;
        if(buttonTexts.Length != 2) 
        {
            Debug.LogError($"[ConfirmationPanelManager] ShowConfirmationPanelWIthFullControl: buttonTexts must have 2 items.");
            return;
        }

        confirmButton.SetActive(!string.IsNullOrWhiteSpace(buttonTexts[0]));
        cancelButton.SetActive(!string.IsNullOrWhiteSpace(buttonTexts[1]));

        titleText.text = displayText;
        // store and add the onClickFunction for the button to listen to.
        currentOnConfirmAction = onConfirmFunction;
        currentOnCancelFunction = onCancelFunction;

        confirmText.text = buttonTexts[0];
        cancelText.text = buttonTexts[1];

        if (string.IsNullOrEmpty(buttonTexts[0])) 
        {
            confirmButton.SetActive(false);
        }
        if (string.IsNullOrEmpty(buttonTexts[1]))
        {
            cancelButton.SetActive(false);
        }

        confirmationPanelContainer.SetActive(true);
    }

    public void OnCancel()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        if (currentOnCancelFunction != null) currentOnCancelFunction();
        currentOnCancelFunction = null;
        // deactivate the panel and get rid of the action that was sent
        confirmationPanelContainer.SetActive(false);   
    }

    public void OnConfirmation()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        // don't need to null check this as it will always receive a confirmation action
        currentOnConfirmAction();
        confirmationPanelContainer.SetActive(false);
    }
}
