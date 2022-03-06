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

    public List<GameObject> characterBorders;
    public GameObject characterSelectionContainer;

    private void Start()
    {
        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerCharacterSelectionPanel);

        startExpeditionButton.interactable = false;
    }

    public void OnCharacterSelected(GameObject currentCharacterBorder)
    {
        startExpeditionButton.interactable = true;

        foreach (GameObject characterBorder in characterBorders)
        {
            characterBorder.SetActive(characterBorder == currentCharacterBorder);
        }
    }

    public void OnStartExpedition()
    {
        GameManager.Instance.LoadScene(inGameScenes.Map);
    }

    public void ActivateInnerCharacterSelectionPanel(bool activate)
    {
        characterSelectionContainer.SetActive(activate);
    }
}