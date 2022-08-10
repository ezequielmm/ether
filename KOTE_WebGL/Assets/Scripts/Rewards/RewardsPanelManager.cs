using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RewardsPanelManager : MonoBehaviour
{
    public GameObject rewardsContainer, rewardsGivenContainer, chooseCardsContainer;

    [Space(20)] public GameObject rewardItemPrefab;

    public GameObject rewardsContent;
    public TMP_Text buttonText;
    private Action onRewardsDoneAction;

    private void Start()
    {
        GameManager.Instance.EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerChooseCardsPanel);
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.AddListener(ActivateInnerRewardsPanel);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener(SetRewards);
    }

    public void SetRewards(SWSM_RewardsData rewards)
    {
        bool rewardsRemaining = false;
        foreach (RewardItemData rewardItem in rewards.data.data.rewards)
        {
            // if the reward item has been taken, don't show it
            if (rewardItem.taken) continue;
            // else add it to the list
            GameObject currentReward = Instantiate(rewardItemPrefab, rewardsContent.transform);
            currentReward.GetComponent<RewardItem>().PopulateRewardItem(rewardItem);
            rewardsRemaining = true;
        }

        if (rewardsRemaining)
        {
            buttonText.text = "Abandon Loot";
        }
        else
        {
            buttonText.text = "Continue";
        }
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

    private void ActivateInnerRewardsPanel(bool activate, Action onRewardsDone)
    {
        rewardsContainer.SetActive(activate);
        onRewardsDoneAction = onRewardsDone;
    }

    public void OnRewardsButtonClicked()
    {
        if (onRewardsDoneAction != null) onRewardsDoneAction.Invoke();
    }
}