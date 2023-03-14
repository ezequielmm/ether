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

    [Test]
    public void ClearItemsOnPopulate() 
    {
        lootboxPanel.Populate(new List<GearItemData>());
        Assert.AreEqual(0, lootboxPanel.LootContainer.transform.childCount);
    }

    [Test]
    public void CorrectItemCount()
    {
        lootboxPanel.Populate(tempData);
        Assert.AreEqual(1, lootboxPanel.LootContainer.transform.childCount);
    }

    [Test]
    public void PanelToggleOn()
    {
        lootboxPanel.TogglePanel(true);
        Assert.AreEqual(true, lootboxPanel.lootboxPanel.activeSelf);
    }

    [Test]
    public void PanelToggleOff()
    {
        lootboxPanel.TogglePanel(false);
        Assert.AreEqual(false, lootboxPanel.lootboxPanel.activeSelf);
    }
}
