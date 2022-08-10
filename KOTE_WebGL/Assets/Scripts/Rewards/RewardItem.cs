using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class RewardItem : MonoBehaviour
{
    public TMP_Text rewardText;
    public RewardItemType rewardItemType;
    private RewardItemData rewardData;    

    public void PopulateRewardItem(RewardItemData reward)
    {
        rewardItemType = Utils.ParseEnum<RewardItemType>(reward.type);
        rewardData = reward;
        switch (rewardItemType)
        {
            case RewardItemType.cards:
                break;
            case RewardItemType.gold:
                rewardText.text = reward.amount + " gold";
                break;
            case RewardItemType.potion:
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
        GameManager.Instance.EVENT_REWARD_SELECTED.Invoke(rewardData.id);
    }
}