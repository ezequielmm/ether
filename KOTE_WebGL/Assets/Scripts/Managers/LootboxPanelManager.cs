using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class LootboxReward
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("image")] public string Image;
}

public class LootboxPanelManager : MonoBehaviour
{
    [SerializeField] public GameObject lootboxPanel;
    [SerializeField] GameObject GearItemPrefab;
    [SerializeField] public GameObject LootContainer;
    [SerializeField] GameObject ReturnToLootButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;
    public RectTransform RadiantBackground;

    private GameStatuses populatedWithStatus;
    
    public void Populate(List<RewardsLoot> items, GameStatuses newGameStatus)
    {
        populatedWithStatus = newGameStatus;
        
        ClearGear();
        foreach (var item in items)
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

    public void OnContinueButton()
    {
        if (populatedWithStatus == GameStatuses.ScoreBoardAndNextAct)
        {
            GameManager.Instance.LoadSceneNewTest();
            return;
        }

        LoadingManager.Won = true;
        Cleanup.CleanupGame();
    }

    private void AddGearItem(RewardsLoot gear)
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
            continueButtonText.text = populatedWithStatus == GameStatuses.ScoreBoard ? "Collect loot" : "Continue";
        }
        
        if (ReturnToLootButton != null)
            ReturnToLootButton.SetActive(enable);
    }

    public void OpenSquires()
    {
        Application.OpenURL("https://squires.knightsoftheether.com/");
    }
}