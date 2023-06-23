using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LootboxPanelManager : MonoBehaviour
{
    [SerializeField] public GameObject lootboxPanel;
    [SerializeField] GameObject GearItemPrefab;
    [SerializeField] public GameObject LootContainer;
    [SerializeField] GameObject ReturnToLootButton;
    public RectTransform RadiantBackground;

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
        foreach (Transform obj in LootContainer.transform)
        {
            Destroy(obj.gameObject);
        }
    }

    public void OnCollectLoot()
    {
        //GameManager.Instance.LoadScene(inGameScenes.MainMenu);
        LoadingManager.Won = true;
        Cleanup.CleanupGame();
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
        if (enable)
        {
            RadiantBackground.DOLocalRotate(new Vector3(0.0f, 0.0f, 1), 1.0f).SetRelative()
                .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        }
        
        if (ReturnToLootButton != null)
            ReturnToLootButton.SetActive(enable);
    }
}