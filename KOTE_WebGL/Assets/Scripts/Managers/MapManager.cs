using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private  GameObject mapScroll;

    private void Start()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
    }

    private void OnMapIconClicked()
    {
        mapScroll.SetActive(true);
    }

    private void OnNodeDataUpdated(NodeStateData arg0)
    {
        mapScroll.SetActive(false);
    }

    public void OnRoyalHousesButton()
    {
        GameManager.Instance.EVENT_ROYALHOUSES_ACTIVATION_REQUEST.Invoke(true);
       
    }

    public void OnShopButton()
    {
        GameManager.Instance.EVENT_SHOPLOCATION_ACTIVATION_REQUEST.Invoke(true);
    }

    public void LoadCombat()
    {
      //  GameManager.Instance.LoadScene(inGameScenes.Combat);
    }

    public void ShowShop()
    {
    }
}