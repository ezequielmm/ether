using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class RewardsPanelManager : MonoBehaviour
{
    public GameObject rewardsContainer, chooseCardsContainer;
    public ParticleSystem goldEffect;

    [Space(20)] public GameObject rewardItemPrefab;

    [FormerlySerializedAs("rewardsContent")]
    public GameObject rewardsPanel;

    public TMP_Text buttonText;
    private List<RewardItem> _rewardItems;
    private int previousRewardsLength;

    private void Start()
    {
        GameManager.Instance.EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST.AddListener(ActivateCardSelectPanel);
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.AddListener(ActivateRewardsContainer);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener(SetRewards);
        _rewardItems = new List<RewardItem>();
        rewardsContainer.SetActive(false);
        chooseCardsContainer.SetActive(false);
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
            GameObject currentReward = Instantiate(rewardItemPrefab, rewardsPanel.transform);
            RewardItem reward = currentReward.GetComponent<RewardItem>();
            reward.PopulateRewardItem(rewardItem, PlayRewardsEffect, () => { ActivateCardSelectPanel(true); });
            _rewardItems.Add(reward);
            rewardsRemaining = true;
        }

        //TODO set up card rewards once we have messages set up
        GameObject rewardGo = Instantiate(rewardItemPrefab, rewardsPanel.transform);
        RewardItem cardReward = rewardGo.GetComponent<RewardItem>();
        cardReward.PopulateRewardItem(new RewardItemData { type = "cards" }, PlayRewardsEffect,
            () => { ActivateCardSelectPanel(true); });


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

    private void ActivateRewardsContainer(bool activate)
    {
        rewardsContainer.SetActive(activate);
        // deactivate the combat ui
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    public void ActivateCardSelectPanel(bool activate)
    {
        rewardsPanel.SetActive(!activate);
        chooseCardsContainer.SetActive(activate);
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
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Coin Reward");
                break;
        }
    }
}