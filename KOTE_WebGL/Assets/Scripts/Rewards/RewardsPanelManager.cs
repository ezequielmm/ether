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
    public ParticleSystem goldEffect;

    [Space(20)] public GameObject rewardItemPrefab;

    public GameObject rewardsContent;
    public TMP_Text buttonText;
    private List<RewardItem> _rewardItems;
    private int previousRewardsLength;
    private void Start()
    {
        GameManager.Instance.EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST.AddListener(ActivateInnerChooseCardsPanel);
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.AddListener(ActivateInnerRewardsPanel);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener(SetRewards);
        _rewardItems = new List<RewardItem>();
    }

    public void SetRewards(SWSM_RewardsData rewards)
    {
        bool rewardsRemaining = false;
        
        ClearRewardItems();
        foreach (RewardItemData rewardItem in rewards.data.data.rewards)
        {
            // if the reward item has been taken, don't show it
            if (rewardItem.taken) continue;
            // else add it to the list
            GameObject currentReward = Instantiate(rewardItemPrefab, rewardsContent.transform);
            RewardItem reward = currentReward.GetComponent<RewardItem>();
            reward.PopulateRewardItem(rewardItem, PlayRewardsEffect);
            _rewardItems.Add(reward);
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

    private void ClearRewardItems()
    {
        if (_rewardItems.Count == 0) return;
        foreach (RewardItem reward in _rewardItems)
        {
            Destroy(reward.gameObject);
        }

        _rewardItems.Clear();
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

    private void ActivateInnerRewardsPanel(bool activate)
    {
        rewardsContainer.SetActive(activate);
    }

    public void OnRewardsButtonClicked()
    {
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }

    private void PlayRewardsEffect(RewardItemType rewardType)
    {
        switch (rewardType)
        {
            case RewardItemType.gold:
                goldEffect.Play();
                break;
        }
    }
}