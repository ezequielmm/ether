using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum RewardItemType
{
    Cards,
    Coins,
    Potion,
    Trinket,
    Fief
}

public class RewardItem : MonoBehaviour
{
    public TMP_Text rewardText;
    public RewardItemType rewardItemType;

    public void SetRewardTypeProperties(RewardItemType rewardItemType)
    {
        this.rewardItemType = rewardItemType;

        rewardText.text = rewardItemType.ToString();
    }

    public void OnRewardClaimed()
    {
        switch (rewardItemType)
        {
            case RewardItemType.Cards:
                GameManager.Instance.EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST.Invoke(true);
                break;
            case RewardItemType.Coins:
                break;
            case RewardItemType.Potion:
                break;
            case RewardItemType.Trinket:
                break;
            case RewardItemType.Fief:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Debug.Log($"Reward Type: {rewardItemType}");
    }
}