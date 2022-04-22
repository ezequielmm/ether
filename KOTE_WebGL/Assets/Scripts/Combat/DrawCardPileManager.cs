using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawCardPileManager : MonoBehaviour
{

    TextMeshProUGUI energy;
    void Start()
    {
        
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Draw);
    }
}
