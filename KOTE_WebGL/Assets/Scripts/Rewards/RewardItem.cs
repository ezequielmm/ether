using System;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class RewardItem : MonoBehaviour
{
    public TMP_Text rewardText;
    [ReadOnly]
    public RewardItemType rewardItemType;
    public Image rewardImage;
    private RewardItemData rewardData;
    
    // the effect gets passed in from the panel so it persists
    private Action<RewardItemType> onRewardSelected;
    // we also pass in a reference to the tooltip so we can display it if necesary
    private GameObject _tooltipPanel;
    private TMP_Text _tooltipText;

    public void PopulateRewardItem(RewardItemData reward, Action<RewardItemType> onSelected)
    {
        onRewardSelected = onSelected;
        rewardItemType = Utils.ParseEnum<RewardItemType>(reward.type);
        rewardData = reward;
        switch (rewardItemType)
        {
            case RewardItemType.cards:
                break;
            case RewardItemType.gold:
                rewardText.text = reward.amount + " gold";
                rewardImage.sprite = SpriteAssetManager.Instance.GetMiscImage("coin");
                break;
            case RewardItemType.potion:
                RewardPotion potion = reward.potion;
                rewardText.text = potion.name;
                rewardImage.sprite = SpriteAssetManager.Instance.GetPotionImage(potion.potionId);
                break;
            case RewardItemType.trinket:
                break;
            case RewardItemType.fief:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void SetupToolTip(GameObject tooltipPanel, TMP_Text tooltipText)
    {
        _tooltipPanel = tooltipPanel;
        _tooltipText = tooltipText;
    }

    public void OnPointerEnter()
    {
        switch (rewardItemType)
        {
            case RewardItemType.potion:
                _tooltipText.text = rewardData.potion.description;
                _tooltipPanel.SetActive(true);
                break;
        }
    }

    public void OnPointerExit()
    {
        _tooltipPanel.SetActive(false);
    }


    public void OnRewardClaimed()
    {
        Debug.Log("onClickFired");
        onRewardSelected.Invoke(rewardItemType);
        _tooltipPanel.SetActive(false);
        _tooltipText.text = "";
        GameManager.Instance.EVENT_REWARD_SELECTED.Invoke(rewardData.id);
    }
}