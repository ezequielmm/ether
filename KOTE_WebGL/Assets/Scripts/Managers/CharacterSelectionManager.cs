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
        //GameManager.Instance.webRequester.RequestCharacterList();// we are not requesting the list until we have more than one type so for the moment only knight

        GameManager.Instance.EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerCharacterSelectionPanel);
        GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.AddListener(OnExpeditionConfirmed);

        startExpeditionButton.interactable = false;
    }

    private void OnExpeditionConfirmed()
    {
        GameManager.Instance.LoadScene(inGameScenes.Expedition);
    }

    public void OnArmoryButton()
    {
        GameManager.Instance.EVENT_ARMORYPANEL_ACTIVATION_REQUEST.Invoke(true);
        ActivateInnerCharacterSelectionPanel(false);
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
        startExpeditionButton.enabled = false;

        GameManager.Instance.webRequester.RequestStartExpedition("knight");//for the moment this is harcoded

        //TO DO: implement API call expedition passing the character id selected
        /* GameManager.Instance.LoadScene(inGameScenes.Expedition);

         string classSelected = currentCharacter.name.Replace("BT", string.Empty);
         GameManager.Instance.EVENT_CHARACTERSELECTED.Invoke(classSelected);
         PlayerPrefs.SetString("class_selected", classSelected);
         PlayerPrefs.Save();*/
    }

    public void ActivateInnerCharacterSelectionPanel(bool activate)
    {
        characterSelectionContainer.SetActive(activate);
    }
}