using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletsManager : MonoBehaviour
{
    public GameObject walletsContainer;

    private void Start()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerWalletsPanel);
    }

    public void ActivateInnerWalletsPanel(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}