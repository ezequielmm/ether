using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryManager : MonoBehaviour
{
    public TreasuryTabsNavigatorManager tabsNavigatorManager;
    [Header("Treasury Panels")] public Transform nftPanel;
    public Transform cardPanel;
    public Transform powerPanel;
    public Transform armorPanel;

    [Space(20)] public GameObject characterList;

    public GameObject tabContentPrefab;
    public GameObject treasuryCardPrefab;
    public GameObject treasuryNftPrefab;
    public GameObject treasuryContainer;

    private void Start()
    {
        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerTreasuryPanel);

        Button firstCharacterButton = characterList.transform.GetChild(0)?.GetComponent<Button>();
        if (firstCharacterButton != null) firstCharacterButton.onClick?.Invoke();
    }

    public void OnCharacterButton()
    {
        tabsNavigatorManager.SelectFirstTab();

        SetNftPanelContent();
        SetCardPanelContent();
        SetPowerPanelContent();
        SetArmorPanelContent();
    }

    // setting random object for testing, but each panel will eventually be handled differently
    private void SetNftPanelContent()
    {
        DestroyChildren(nftPanel);
        SetObjects(nftPanel, treasuryNftPrefab);
    }

    private void SetCardPanelContent()
    {
        DestroyChildren(cardPanel);
        SetObjects(cardPanel, treasuryCardPrefab);
    }

    private void SetPowerPanelContent()
    {
        DestroyChildren(powerPanel);
        SetObjects(powerPanel, tabContentPrefab);
    }

    private void SetArmorPanelContent()
    {
        DestroyChildren(armorPanel);
        SetObjects(armorPanel, tabContentPrefab);
    }

    public void DestroyChildren(Transform transform)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetObjects(Transform transform, GameObject contentPrefab)
    {
        int objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(contentPrefab, transform);
            TMP_Text objectText = localObject.GetComponent<TMP_Text>();
            if (objectText != null) objectText.text = i.ToString();
        }
    }

    public void OnWalletsButton()
    {
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerTreasuryPanel(bool activate)
    {
        treasuryContainer.SetActive(activate);
    }
}