using KOTE.UI.Armory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootboxPanelManager : MonoBehaviour
{
    [SerializeField] GameObject lootboxPanel;
    [SerializeField] GameObject GearItemPrefab;
    [SerializeField] GameObject LootContainer;
    [SerializeField] GameObject ReturnToLootButton;

    public void Populate(List<GearItemData> items)
    {
        ClearGear();
        foreach (GearItemData item in items) 
        {
            AddGearItem(item);
        }
    }

    private void ClearGear() 
    {
        foreach(Transform obj in LootContainer.transform) 
        {
            Destroy(obj.gameObject);
        }
    }

    private void AddGearItem(GearItemData gear) 
    {
        GameObject GearItemObject = Instantiate(GearItemPrefab, LootContainer.transform);
        GearItem gearItem = GearItemObject.GetComponent<GearItem>();
        gearItem.Populate(gear);
    }

    public void TogglePanel(bool enable)
    {
        lootboxPanel.SetActive(enable);
        ReturnToLootButton.SetActive(enable);
    }
}
