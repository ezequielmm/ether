using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnvironmentManager : MonoBehaviour
{
    public Image combatBg; 

    void Start()
    {
      //  GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
       // GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(OnMapNodesUpdate);
      //  GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnMapNodeSelected);
    }


    private void onMapNodeSelected(int arg0)
    {
    }

    private void onMapNodesUpdate(string arg0)
    {
    }

    private void OnMapIconClicked()
    {
    }

 
}
