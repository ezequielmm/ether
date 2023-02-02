using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectTrinketPanel : CardPanelBase
{
    public GameObject objectLayout;
    public GameObject trinketPrefab;
    public Button selectButton;
    public Button backButton;

    private int trinketsToSelect = 0;
    private int trinketsSelected = 0;
    private List<string> selectedTrinketIds = new List<string>();
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameManager.Instance.EVENT_SHOW_SELECT_TRINKET_PANEL.AddListener(ShowTrinkets);
    }

    private void ShowTrinkets(SWSM_SelectTrinketData trinketData)
    {
        SetupPanel();
        trinketsToSelect = 1;
        foreach (Trinket trinket in trinketData.data.data.trinkets)
        {
            GameObject currentReward = Instantiate(trinketPrefab, objectLayout.gameObject.transform);
            TrinketItemManager trinketManager = currentReward.GetComponent<TrinketItemManager>();
            
            // this gets tricky because we have to use IPointerClickHandler instead of Toggle component
            // due to conflicts with the tooltips, so there's a bit more logic here
            trinketManager.Populate(trinket, true, (isToggled) =>
            {
                if (trinketsSelected >= trinketsToSelect)
                {
                    trinketManager.isToggled = false;
                }
                if (isToggled && trinketsSelected < trinketsToSelect)
                {
                    trinketManager.isToggled = true;
                    trinketsSelected++;
                    selectedTrinketIds.Add(trinketManager.Id);
                }
                else if (!isToggled)
                {
                    trinketManager.isToggled = false;
                    trinketsSelected--;
                    selectedTrinketIds.Remove(trinketManager.Id);
                }
                selectButton.interactable = (trinketsSelected == trinketsToSelect);
                trinketManager.UpdateToggleStatus();
            });
        }
        
        ShowPanel();
    }

    private void SetupPanel()
    {
        DestroyCards();
        selectedTrinketIds.Clear();
        gridLayout.cellSize = new Vector2(250, 250);
    }

    private void ShowPanel()
    {
        selectButton.onClick.AddListener(OnSelectClicked);
        backButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(true);
        commonCardsContainer.SetActive(true);
    }

    private void OnSelectClicked()
    {
        GameManager.Instance.EVENT_TRINKETS_SELECTED.Invoke(selectedTrinketIds);
        
        selectedTrinketIds.Clear();
        trinketsSelected = 0;
        trinketsToSelect = 0;
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(HideCardSelectPanel);
        selectButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
        HideCardSelectPanel();
        
    }
}
