using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WalletManager : MonoBehaviour
{
    public GameObject walletsContainer;
    public GameObject confirmationPanel;

    [Space(20)]
    public GameObject informationContent;

    public Color darkRowColor, lightRowColor;
    public GameObject rowPrefab;

    public GameObject walletToDelete;

    private void Start()
    {
        walletsContainer.GetComponentInChildren<ScrollRect>().scrollSensitivity = GameSettings.PANEL_SCROLL_SPEED;
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerWalletsPanel);
        GameManager.Instance.EVENT_DISCONNECT_WALLET_PANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerDisconnectWalletConfirmPanel);
        GameManager.Instance.EVENT_WALLET_CONTENTS_RECEIVED.AddListener(OnWalletDataReceived);
        SetInformationRows();
    }

    public void OnWalletDataReceived()
    {
        int[] tokenIds = { 1, 219, 170 };
        
    }

    public void SetInformationRows()
    {
        int randomRows = Random.Range(10, 25);
        for (int i = 0; i < randomRows; i++)
        {
            GameObject currentRow = Instantiate(rowPrefab, informationContent.transform);
            currentRow.GetComponent<WalletItem>().SetColor(i % 2 == 0 ? lightRowColor : darkRowColor);
        }
    }

    public void OnDisconnectConfirm()
    {
        Destroy(walletToDelete);

        for (int i = 0; i < informationContent.transform.childCount; i++)
        {
            informationContent.transform.GetChild(i).GetComponent<WalletItem>().SetColor(i % 2 != 0 ? lightRowColor : darkRowColor);
        }
    }

    public void ActivateInnerDisconnectWalletConfirmPanel(bool activate, GameObject wallet)
    {
        if (wallet != null) walletToDelete = wallet;
        confirmationPanel.SetActive(activate);
    }

    public void ActivateInnerDisconnectWalletConfirmPanel(bool activate)
    {
        confirmationPanel.SetActive(activate);
    }

    public void ActivateInnerWalletsPanel(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}