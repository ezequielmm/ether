using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GameManager.Instance.EVENT_MOVE_CARD.AddListener(OnCardToMove);
    }

    private void OnCardToMove(CardToMoveData data)
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
