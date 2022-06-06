using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnvironmentManager : MonoBehaviour
{
    public Image mapBg; 
    public Image combatBg; 

    void Start()
    {
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
        GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(onMapNodesUpdate);
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(onMapNodeSelected);
        mapBg.gameObject.SetActive(false);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
