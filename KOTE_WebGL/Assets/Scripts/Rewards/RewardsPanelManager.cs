using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardsPanelManager : MonoBehaviour
{
    public GameObject rewardsContainer, rewardsGivenContainer, chooseCardsContainer;

    [Space(20)]
    public GameObject rewardItemPrefab;

    public GameObject rewardsContent;

    private void Start()
    {
        GameManager.Instance.EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerChooseCardsPanel);
        GameManager.Instance.EVENT_REWARDSPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerRewardsPanel);

        SetRewards();
    }

    public void SetRewards()
    {
        for (int i = 0; i < 3; i++)
        {
            int randomReward = Random.Range(0, Enum.GetValues(typeof(RewardItemType)).Length);

            GameObject currentReward = Instantiate(rewardItemPrefab, rewardsContent.transform);
            currentReward.GetComponent<RewardItem>().SetRewardTypeProperties((RewardItemType) randomReward);
        }

        GameObject cardsReward = Instantiate(rewardItemPrefab, rewardsContent.transform);
        cardsReward.GetComponent<RewardItem>().SetRewardTypeProperties(RewardItemType.Cards);
    }

    public void ActivateInnerRewardsGivenPanel(bool activate)
    {
        rewardsGivenContainer.SetActive(activate);
    }

    public void ActivateInnerChooseCardsPanel(bool activate)
    {
        ActivateInnerRewardsGivenPanel(!activate);
        chooseCardsContainer.SetActive(activate);
    }

    public void ActivateInnerRewardsPanel(bool activate)
    {
        rewardsContainer.SetActive(activate);
    }
}