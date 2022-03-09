using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WalletManager : MonoBehaviour
{
    public GameObject walletsContainer;
    public GameObject confirmationPanel;

    [Space(20)]
    public GameObject informationContent;

    public Color darkRowColor, lightRowColor;
    public GameObject rowPrefab;

    private void Start()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerWalletsPanel);
        GameManager.Instance.EVENT_DISCONNECTWALLETPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerDisconnectWalletConfirmPanel);

        SetInformationRows();
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

    public void ActivateInnerDisconnectWalletConfirmPanel(bool activate)
    {
        confirmationPanel.SetActive(activate);
    }

    public void ActivateInnerWalletsPanel(bool activate)
    {
        walletsContainer.SetActive(activate);
    }
}