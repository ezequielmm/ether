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
        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/RewardsPanel.prefab");
        GameObject drawPileManager = Instantiate(drawPilePrefab);
        _rewardsPanelManager = drawPileManager.GetComponent<RewardsPanelManager>();
        drawPileManager.SetActive(true);

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
}