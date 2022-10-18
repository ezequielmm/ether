using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantNodeManager : MonoBehaviour
{
    public GameObject shopContainer;

    [Header("Shop Panels")] 
    public Transform cardPanel;
    public Transform potionPanel;
    public Transform trinketPanel;

    [Header("Item Prefabs")]
    public GameObject uiCardPrefab;
    public GameObject uiPotionPrefab;
    public GameObject uiTrinketPrefab;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_MERCHANT_PANEL.AddListener(ActivateInnerMerchantPanel);
    }

    private void ActivateInnerMerchantPanel()
    {
        shopContainer.SetActive(true);
    }
    
    public void CloseMerchantPanel()
    {
        shopContainer.SetActive(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}