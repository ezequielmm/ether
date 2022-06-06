using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void EndTurn()
    {
        GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
    }
}
