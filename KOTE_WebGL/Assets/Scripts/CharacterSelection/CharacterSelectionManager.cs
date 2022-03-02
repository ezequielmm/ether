using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    public Button startExpeditionButton;

    private bool characterSelected;

    private void Start()
    {
        startExpeditionButton.interactable = false;
    }

    private void Update()
    {
        OnCharacterSelected();
    }

    public void OnCharacterSelected()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            startExpeditionButton.interactable =
                EventSystem.current.currentSelectedGameObject.GetComponent<CharacterToSelect>() != null ||
                EventSystem.current.currentSelectedGameObject == startExpeditionButton.gameObject;
        }
        else
        {
            startExpeditionButton.interactable = false;
        }
    }

    public void OnStartExpedition()
    {
        // GameManager.Instance.LoadScene();
    }
}