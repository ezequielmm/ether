using System.Collections;
using System.Collections.Generic;
using KOTE.UI.Armory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ArmoryPanelManagerTests : MonoBehaviour
{
    
    private List<Nft> testNftList = new()
    {
        new Nft()
        {
            ImageUrl = "test.com",
            TokenId = 0,
            Traits = new Dictionary<Trait, string>()
                {
                    { Trait.Helmet, "helmet" }
                }
        },
        new Nft()
        {
            ImageUrl = "nope",
            TokenId = 9999,
            Traits = new Dictionary<Trait, string>()
                {
                    { Trait.Boots, "boots" }
                }
        }
    };

    private Image characterImage;
    private GameObject armoryPanel;
    private ArmoryPanelManager _armoryPanelManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/NftSpriteManager.prefab");
        GameObject nftSpriteManager = Instantiate(spriteManagerPrefab);
        nftSpriteManager.SetActive(true);

        GameObject armoryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/ArmoryPanel.prefab");
        armoryPanel = Instantiate(armoryPrefab);
        _armoryPanelManager = armoryPanel.GetComponent<ArmoryPanelManager>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameManager.Instance.DestroyInstance();
        Destroy(armoryPanel);
        _armoryPanelManager = null;
        yield return null;
    }

    [Test]
    public void DoesCallingShowArmoryPanelActivateArmoryPanel()
    {
        Assert.IsFalse(_armoryPanelManager.panelContainer.activeSelf);
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
        Assert.IsTrue(_armoryPanelManager.panelContainer.activeSelf);
    }

    [Test]
    public void DoesCallingShowArmoryPanelDeactivateArmoryPanel()
    {
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
        Assert.IsTrue(_armoryPanelManager.panelContainer.activeSelf);
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(false);
        Assert.IsFalse(_armoryPanelManager.panelContainer.activeSelf);
    }

    [Test]
    public void DoesOnBackButtonHideArmoryPanel()
    {
        GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
        Assert.IsTrue(_armoryPanelManager.panelContainer.activeSelf);
        _armoryPanelManager.OnBackButton();
        Assert.IsFalse(_armoryPanelManager.panelContainer.activeSelf);
    }
}