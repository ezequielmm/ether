using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentManager : MonoBehaviour
{
    public Image mapBg; 

    void Start()
    {
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);
      //  GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);
       // mapBg.gameObject.SetActive(false);
    }

    private void OnNodeDataUpdated(string arg0)
    {
       // mapBg.gameObject.SetActive(false);
    }

    private void OnMapIconClicked()
    {
      //  mapBg.gameObject.SetActive(mapBg.gameObject.activeSelf ? false:true) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
