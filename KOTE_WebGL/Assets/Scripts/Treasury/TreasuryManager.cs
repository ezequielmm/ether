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
        ScrollRect[] scrollList = treasuryContainer.GetComponentsInChildren<ScrollRect>();
        foreach (ScrollRect scrollRect in scrollList)
        {
            scrollRect.scrollSensitivity = GameSettings.PANEL_SCROLL_SPEED;
        }

        GameManager.Instance.EVENT_TREASURYPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerTreasuryPanel);
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(SetNftPanelContent);
        GameManager.Instance.EVENT_REQUEST_NFT_METADATA.AddListener(OnMetadataRequested);
        Button firstCharacterButton = characterList.transform.GetChild(0)?.GetComponent<Button>();
        if (firstCharacterButton != null) firstCharacterButton.onClick?.Invoke();
    }

    public void OnCharacterButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        tabsNavigatorManager.SelectFirstTab();

        SetCardPanelContent();
        SetPowerPanelContent();
        SetArmorPanelContent();
    }
    // when a new wallet is received clear the panel
    private void OnMetadataRequested(int[] ids)
    {
        for (int i = 0; i < nftPanel.transform.childCount; i++)
        {
            Destroy(nftPanel.transform.GetChild(i).gameObject);
        }
    }
    
    
    private void SetNftPanelContent(NftData heldNftData)
    {
        foreach (NftMetaData metaData in heldNftData.assets)
        {
            GameObject localObject = Instantiate(treasuryNftPrefab, nftPanel);
            localObject.GetComponent<TreasuryNftItem>().Populate(metaData);
        }
    }

    // setting random object for testing, but each panel will eventually be handled differently
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

    public void SetObjects(Transform panelTransform, GameObject contentPrefab)
    {
        int objectsAmount = Random.Range(7, 20);

        for (int i = 0; i < objectsAmount; i++)
        {
            GameObject localObject = Instantiate(contentPrefab, panelTransform);
            TMP_Text objectText = localObject.GetComponent<TMP_Text>();
            if (objectText != null) objectText.text = i.ToString();
        }
    }

    public void OnWalletsButton()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        GameManager.Instance.EVENT_WALLETSPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void ActivateInnerTreasuryPanel(bool activate)
    {
        treasuryContainer.SetActive(activate);
    }
}