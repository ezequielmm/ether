using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class RewardItem : MonoBehaviour
{
    public TMP_Text rewardText;
    [ReadOnly]
    public RewardItemType rewardItemType;
    public Image rewardImage;
    private RewardItemData rewardData;
    // the effect gets passed in from the panel so it persists
    private Action<RewardItemType> onRewardSelected;

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
                rewardText.text = "potion";
                rewardImage.sprite = SpriteAssetManager.Instance.GetPotionImage(reward.amount);
                break;
            case RewardItemType.trinket:
                break;
            case RewardItemType.fief:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void OnRewardClaimed()
    {
        Debug.Log("onClickFired");
        onRewardSelected.Invoke(rewardItemType);
        GameManager.Instance.EVENT_REWARD_SELECTED.Invoke(rewardData.id);
    }
}