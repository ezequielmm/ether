using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
public class BlessingItemTests : MonoBehaviour
{
    private BlessingItem _blessingItem;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject BlessingItemPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/RoyalHouse/BlessingItemPrefab.prefab");
        GameObject blessingItemInstance = Instantiate(BlessingItemPrefab);
        _blessingItem = blessingItemInstance.GetComponent<BlessingItem>();
        blessingItemInstance.SetActive(true);
        yield return null;
    }
    
    
    [Test]
    public void DoesSettingPropertiesUpdateTextFields()
    {
        _blessingItem.SetProperties("name", "description", 1);
        Assert.AreEqual("name", _blessingItem.itemNameText.text);
        Assert.AreEqual("description", _blessingItem.itemDescriptionText.text);
        Assert.AreEqual("1 $fief", _blessingItem.itemFiefText.text);
    }

    [Test]
    public void DoesOnArmorySelectedToggleSelected()
    {
        _blessingItem.OnBlessingItemSelected();
        Assert.True(_blessingItem.selected);
        _blessingItem.OnBlessingItemSelected();
        Assert.False(_blessingItem.selected);
    }

    [Test]
    public void DoesOnArmorySelectedFireArmorySelectEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SELECTBLESSING_ITEM.AddListener((data, data2) => { eventFired = true;});
        _blessingItem.OnBlessingItemSelected();
        Assert.True(eventFired);
    }
}
