using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBottomUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

    private void Start()
    {
        container.SetActive(false);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);//TODO: this will change as not all nodes will be combat
        GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.AddListener(OnMapPanelToogle);
}

    private void OnMapPanelToogle(bool arg0)
    {
        //we need to hide or content when the map panel is on
        container.SetActive(!arg0);
    }

    private void OnNodeDataUpdated(NodeStateData nodeData, bool initialCall)
    {
        if (initialCall)
        {
            container.SetActive(true);
        }
       
    }

}
