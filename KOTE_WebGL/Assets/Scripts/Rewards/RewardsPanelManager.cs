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

    public GameObject cardRewardLayout;
    public SelectableUiCardManager cardPrefab;

    public TMP_Text buttonText;
    private List<RewardItem> _rewardItems;
    private List<SelectableUiCardManager> _cardRewards;
    private int previousRewardsLength;

    private void Start()
    {
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.AddListener(ActivateRewardsContainer);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener(SetRewards);
        _rewardItems = new List<RewardItem>();
        _cardRewards = new List<SelectableUiCardManager>();
        rewardsContainer.SetActive(false);
        chooseCardsContainer.SetActive(false);
    }

    public void SetRewards(SWSM_RewardsData rewards)
    {
        bool rewardsRemaining = false;

        ClearRewardItems();
        ClearRewardCards();
        foreach (RewardItemData rewardItem in rewards.data.data.rewards)
        {
            // if the reward item has been taken, don't show it
            if (rewardItem.taken) continue;

            //if it's a card, create it on the card display instead
            if (rewardItem.type == "card")
            {
                AddCardToRewards(rewardItem);
                rewardsRemaining = true;
                continue;
            }

            AddReward(rewardItem);
            rewardsRemaining = true;
        }

        // handle the game reward ourselves, so that it opens the card select window
        if (_cardRewards.Count > 0)
        {
            ShowCardRewardItem();
        }

        SetRewardButtonText(rewardsRemaining);
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

    private void AddReward(RewardItemData rewardItem)
    {
        // else add it to the list
        GameObject currentReward = Instantiate(rewardItemPrefab, rewardsPanel.transform);
        RewardItem reward = currentReward.GetComponent<RewardItem>();
        reward.PopulateRewardItem(rewardItem, PlayRewardsEffect);
        _rewardItems.Add(reward);
    }

    private void AddCardToRewards(RewardItemData reward)
    {
        SelectableUiCardManager newCard = Instantiate(cardPrefab, cardRewardLayout.transform);
        newCard.Populate(reward.card);
        newCard.cardSelectorToggle.onValueChanged.AddListener((isOn) =>
        {
            GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
            GameManager.Instance.EVENT_REWARD_SELECTED.Invoke(reward.id);
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Reward");
            ActivateCardSelectPanel(false);
        });
        _cardRewards.Add(newCard);
    }

    private void ShowCardRewardItem()
    {
        GameObject cardRewardObject = Instantiate(rewardItemPrefab, rewardsPanel.transform);
        RewardItem cardReward = cardRewardObject.GetComponent<RewardItem>();
        cardReward.PopulateRewardItem(new RewardItemData
        {
            type = "card"
        }, PlayRewardsEffect, () => { ActivateCardSelectPanel(true); });
        _rewardItems.Add(cardReward);
    }

    private void ClearRewardCards()
    {
        if (_cardRewards.Count == 0) return;
        foreach (SelectableUiCardManager card in _cardRewards)
        {
            Destroy(card.gameObject);
        }

        _cardRewards.Clear();
    }

    private void SetRewardButtonText(bool rewardsRemaining)
    {
        if (rewardsRemaining)
        {
            buttonText.text = "Abandon Loot";
        }
        else
        {
            buttonText.text = "Continue";
        }
    }

    private void ActivateRewardsContainer(bool activate)
    {
        rewardsContainer.SetActive(activate);
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
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Coin Reward");
                break;
            default:
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
                break;
        }
    }
}