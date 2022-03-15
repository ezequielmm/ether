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

    private GameObject currentCharacter;

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

    public void SetSelectedCharacter(GameObject selectedCharacter)
    {
        currentCharacter = selectedCharacter;
    }

    public void OnStartExpedition()
    {
        GameManager.Instance.LoadScene(inGameScenes.Map);

        string classSelected = currentCharacter.name.Replace("BT", string.Empty);
        GameManager.Instance.EVENT_CHARACTERSELECTED.Invoke(classSelected);
    }

    public void ActivateInnerCharacterSelectionPanel(bool activate)
    {
        characterSelectionContainer.SetActive(activate);
    }
}