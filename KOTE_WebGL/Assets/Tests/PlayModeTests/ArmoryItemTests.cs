using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class ArmoryItemTests : MonoBehaviour
{
    private ArmoryItem _armoryItem;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject ArmoryItemPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/RoyalHouse/RoyalHouseItemPrefab.prefab");
        GameObject armoryItemInstance = Instantiate(ArmoryItemPrefab);
        _armoryItem = armoryItemInstance.GetComponent<ArmoryItem>();
        armoryItemInstance.SetActive(true);
        yield return null;
    }

    [Test]
    public void DoesSettingPropertiesUpdateTextFields()
    {
        _armoryItem.SetProperties("name", "description", 1);
        Assert.AreEqual("name", _armoryItem.itemNameText.text);
        Assert.AreEqual("description", _armoryItem.itemDescriptionText.text);
        Assert.AreEqual("Encumbrance: 1", _armoryItem.itemEncumbranceText.text);
    }

    [Test]
    public void DoesOnArmorySelectedToggleSelected()
    {
        _armoryItem.OnArmoryItemSelected();
        Assert.True(_armoryItem.selected);
        _armoryItem.OnArmoryItemSelected();
        Assert.False(_armoryItem.selected);
    }

    [Test]
    public void DoesOnArmorySelectedFireArmorySelectEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SELECTARMORY_ITEM.AddListener((data, data2) => { eventFired = true;});
        _armoryItem.OnArmoryItemSelected();
        Assert.True(eventFired);
    }
}