using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCardsPanel : MonoBehaviour
{
    public GameObject CommonCardsContainer;
    void Start()
    {
        CommonCardsContainer.SetActive(false);
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener(DisplayCards);
    }

    private void DisplayCards(PileTypes pileType)
    {
        if (CommonCardsContainer.activeSelf)
        {
            CommonCardsContainer.SetActive(false);
        }
        else
        {
            CommonCardsContainer.SetActive(true);
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
