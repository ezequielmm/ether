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
    }
}