using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantNodeManager : MonoBehaviour
{
    public GameObject shopLocationContainer;

    [Header("Shop Layouts")] 
    public GameObject cardLayout;
    public GameObject trinketLayout;
    public GameObject potionLayout;
    public GameObject gearLayout;
    
    [Header("Item Prefabs")]
    public GameObject merchantCardPrefab;
    public GameObject merchantItemPrefab;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_MERCHANT_PANEL.AddListener(ActivateInnerMerchantPanel);
    }

    private void ActivateInnerMerchantPanel()
    {
        shopLocationContainer.SetActive(true);
    }

    public void CloseMerchantPanel()
    {
        shopLocationContainer.SetActive(false);
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}