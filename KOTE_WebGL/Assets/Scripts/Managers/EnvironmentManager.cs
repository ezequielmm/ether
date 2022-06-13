using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnvironmentManager : MonoBehaviour
{
    public Image mapBg; 
    public Image combatBg; 

    void Start()
    {
      //  GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
       // GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(OnMapNodesUpdate);
      //  GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnMapNodeSelected);
        GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.AddListener(OnToogleMap);
       // mapBg.gameObject.SetActive(false);
    }

    private void OnToogleMap(bool data)
    {
        mapBg.gameObject.SetActive(data);
    }

    private void onMapNodeSelected(int arg0)
    {
        mapBg.gameObject.SetActive(false);
      //  mapBg.DOFade
    }

    private void onMapNodesUpdate(string arg0)
    {
        mapBg.gameObject.SetActive(true);
    }

    private void OnMapIconClicked()
    {
       mapBg.gameObject.SetActive(mapBg.gameObject.activeSelf ? false:true) ;
    }

 
}
