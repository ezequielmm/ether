using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnergyCounterManager : MonoBehaviour
{
    public TextMeshProUGUI energyTF;

    private void Awake()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
    }

    private void Start()
    {
       // GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE quertyType)
    {
        if(nodeState.data != null && nodeState.data.data != null)energyTF.SetText(nodeState.data.data.player.energy + "/" + nodeState.data.data.player.energy_max);
    }

 
}