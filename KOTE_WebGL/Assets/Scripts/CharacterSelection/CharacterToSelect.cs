using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterToSelect : MonoBehaviour
{
    public GameObject border;

    private void Start()
    {
        border.SetActive(false);
    }

    private void Update()
    {
        if (gameObject == EventSystem.current.currentSelectedGameObject)
        {
            OnCharacterSelected();
        }
        else
        {
            OnCharacterUnselected();
        }
    }

    public void OnCharacterSelected()
    {
        border.SetActive(true);
    }

    public void OnCharacterUnselected()
    {
        border.SetActive(false);
    }
}