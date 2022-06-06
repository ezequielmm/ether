using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private GameObject combatContainer;

    private void Start()
    {
        combatContainer.SetActive(false);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);//TODO: this will change as not all nodes will be combat
    }

    private void OnNodeDataUpdated(NodeStateData nodeState,WS_QUERY_TYPE wsType)
    {
        if(wsType == WS_QUERY_TYPE.MAP_NODE_SELECTED)combatContainer.SetActive(true);
    }
}