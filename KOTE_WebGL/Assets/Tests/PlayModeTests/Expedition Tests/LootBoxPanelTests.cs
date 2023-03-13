using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class LootBoxPanelTests : MonoBehaviour
{
    LootboxPanelManager lootboxPanel;
    List<GearItemData> tempData = new List<GearItemData>()
    {
        new GearItemData()
        {
            category = "Gauntlet",
            gearId = 1,
            name = "Test",
            trait = "Gauntlet"
        }
    };

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameObject Prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/LootboxPanel.prefab");
        GameObject Object = Instantiate(Prefab);
        lootboxPanel = Object.GetComponent<LootboxPanelManager>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(lootboxPanel.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ClearItemsOnPopulate() 
    {
        lootboxPanel.Populate(new List<GearItemData>());
        yield return null;
        Assert.AreEqual(0, lootboxPanel.LootContainer.transform.childCount);
    }

    [UnityTest]
    public IEnumerator CorrectItemCount()
    {
        lootboxPanel.Populate(tempData);
        yield return null;
        Assert.AreEqual(1, lootboxPanel.LootContainer.transform.childCount);
    }

    [UnityTest]
    public IEnumerator PanelToggleOn()
    {
        lootboxPanel.TogglePanel(true);
        yield return null;
        Assert.AreEqual(true, lootboxPanel.lootboxPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator PanelToggleOff()
    {
        lootboxPanel.TogglePanel(false);
        yield return null;
        Assert.AreEqual(false, lootboxPanel.lootboxPanel.activeSelf);
    }
}
