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

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        GameObject spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/RewardsPanel.prefab");
        GameObject rewardPanelManager = Instantiate(drawPilePrefab);
        _rewardsPanelManager = rewardPanelManager.GetComponent<RewardsPanelManager>();
        rewardPanelManager.SetActive(true);
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

    [UnityTest]
    public IEnumerator DoesPopulatingRewardsCreateRewardItems()
    {
        yield return null;
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(new SWSM_RewardsData
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
        });
        Assert.AreEqual(1, _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>().Length);
    }


    [Test]
    public void DoesSettingRewardsClearRewardItems()
    {
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(new SWSM_RewardsData
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
        });
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
        Assert.AreEqual(0, _rewardsPanelManager.rewardsPanel.GetComponentsInChildren<RewardItem>().Length);

    }
    
}