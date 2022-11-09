using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class RewardsPanelManagerTests : MonoBehaviour
{
    private RewardsPanelManager _rewardsPanelManager;
    private GameObject spriteManager;

    private readonly SWSM_RewardsData _coinRewardData = new SWSM_RewardsData
    {
        data = new SWSM_RewardsData.Data
        {
            data = new SWSM_RewardsData.Data.RewardsData
            {
                rewards = new List<RewardItemData>
                {
                    new RewardItemData
                    {
                        amount = 1,
                        taken = false,
                        type = "gold"
                    }
                }
            }
        }
    };

    private readonly SWSM_RewardsData _cardRewardData = new SWSM_RewardsData
    {
        data = new SWSM_RewardsData.Data
        {
            data = new SWSM_RewardsData.Data.RewardsData
            {
                rewards = new List<RewardItemData>
                {
                    new RewardItemData
                    {
                        amount = 1,
                        taken = false,
                        type = "card",
                        card = new Card
                        {
                            cardId = 5,
                            cardType = "attack",
                            description = "test",
                            energy = 3,
                            id = "test",
                            isUpgraded = false,
                            keywords = new List<string>(),
                            name = "test",
                            pool = "knight",
                            properties = new Effects(),
                            rarity = "legendary"
                        }
                    }
                }
            }
        }
    };

    private readonly SWSM_RewardsData _emptyRewardData = new SWSM_RewardsData
    {
        data = new SWSM_RewardsData.Data
        {
            data = new SWSM_RewardsData.Data.RewardsData
            {
                rewards = new List<RewardItemData>()
            }
        }
    };

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/RewardsPanel.prefab");
        GameObject rewardPanelManager = Instantiate(drawPilePrefab);
        _rewardsPanelManager = rewardPanelManager.GetComponent<RewardsPanelManager>();
        rewardPanelManager.SetActive(true);
        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_rewardsPanelManager.gameObject);
        Destroy(spriteManager);
        yield return null;
    }

    [Test]
    public void IsRewardsContainerDeactiavatedOnStart()
    {
        Assert.False(_rewardsPanelManager.rewardsContainer.activeSelf);
    }

    [Test]
    public void DoesCallingShowRewardsManagerShowRewardsPanel()
    {
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
        Assert.True(_rewardsPanelManager.rewardsContainer.activeSelf);
    }

    [Test]
    public void DoesCallingShowRewardsMangerDeactivateCombatElements()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesPopulatingRewardsCreateRewardItems()
    {
        yield return null;
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_coinRewardData);
        Assert.AreEqual(1, _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>().Length);
    }


    [UnityTest]
    public IEnumerator DoesSettingRewardsClearRewardItems()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_coinRewardData);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_emptyRewardData);
        yield return null;
        Assert.AreEqual(0, _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>().Length);
    }

    [Test]
    public void DoesPopulatingRewardsWithACardShowCardRewardItem()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_cardRewardData);
        Assert.AreEqual(1, _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>().Length);
        Assert.AreEqual(RewardItemType.card,
            _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>()[0].rewardItemType);
    }

    [UnityTest]
    public IEnumerator DoesPopulatingRewardsWithCardsCreateCardRewards()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_cardRewardData);
        yield return null;
        Assert.AreEqual(1,
            _rewardsPanelManager.cardRewardLayout.GetComponentsInChildren<SelectableUiCardManager>().Length);
    }

    [UnityTest]
    public IEnumerator DoesPopulatingRewardsClearCardRewards()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_cardRewardData);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(new SWSM_RewardsData
        {
            data = new SWSM_RewardsData.Data
            {
                data = new SWSM_RewardsData.Data.RewardsData
                {
                    rewards = new List<RewardItemData>()
                }
            }
        });
        yield return null;
        Assert.AreEqual(0,
            _rewardsPanelManager.cardRewardLayout.GetComponentsInChildren<SelectableUiCardManager>().Length);
    }

    [UnityTest]
    public IEnumerator DoesPopulatingRewardsChangeRewardButtonText()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_coinRewardData);
        yield return null;
        Assert.AreEqual("Abandon Loot", _rewardsPanelManager.buttonText.text);
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_emptyRewardData);
        yield return null;
        Assert.AreEqual("Continue", _rewardsPanelManager.buttonText.text);
    }

    [Test]
    public void DoesClickingCardRewardFireRewardSelectedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_REWARD_SELECTED.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_cardRewardData);
        _rewardsPanelManager.cardRewardLayout.GetComponentsInChildren<SelectableUiCardManager>()[0].cardSelectorToggle
            .onValueChanged.Invoke(true);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingCardRewardDeactivateCardSelectPanel()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_cardRewardData);
        _rewardsPanelManager.cardRewardLayout.GetComponentsInChildren<SelectableUiCardManager>()[0].cardSelectorToggle
            .onValueChanged.Invoke(true);
        Assert.False(_rewardsPanelManager.chooseCardsContainer.activeSelf);
    }

    [Test]
    public void DoesActivatingCardSelectPanelHideRewardsPanel()
    {
        _rewardsPanelManager.ActivateCardSelectPanel(true);
        Assert.False(_rewardsPanelManager.rewardsPanel.activeSelf);
    }

    [Test]
    public void DoesActivatingCardSelectPanelShowCardSelectPanel()
    {
        _rewardsPanelManager.ActivateCardSelectPanel(true);
        Assert.True(_rewardsPanelManager.chooseCardsContainer.activeSelf);
    }

    [Test]
    public void DoesDeactivatingCardSelectPanelShowRewardsPanel()
    {
        _rewardsPanelManager.ActivateCardSelectPanel(false);
        Assert.True(_rewardsPanelManager.rewardsPanel.activeSelf);
    }

    [Test]
    public void DoesDeactivatingCardSelectPanelHideCardSelectPanel()
    {
        _rewardsPanelManager.ActivateCardSelectPanel(false);
        Assert.False(_rewardsPanelManager.chooseCardsContainer.activeSelf);
    }

    [Test]
    public void DoesClickingOnContinueButtonFireContinueExpeditionEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.AddListener(() => { eventFired = true;});
        _rewardsPanelManager.OnRewardsButtonClicked();
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesClickingCoinRewardFirePlaySFXEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data) => { eventFired = true;});
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(_coinRewardData);
        RewardItem coinReward = _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>()[0];
        coinReward.OnRewardClaimed();
        yield return null;
        Assert.True(eventFired);
    }
}