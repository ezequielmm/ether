using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
public class TrinketsHolderManagerTests : MonoBehaviour
{
    private TrinketsHolderManager trinketManager;
    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject trinketHolderPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/TrinketsHolder.prefab");
        GameObject trinketHolderInstance = Instantiate(trinketHolderPrefab);
        trinketManager = trinketHolderInstance.GetComponent<TrinketsHolderManager>();
        trinketHolderInstance.SetActive(true);
        yield return null;
    }

    [Test]
    public void DoesStartDisableTrinketContainer()
    {
        Assert.False(trinketManager.trinketsContainer.activeSelf);
    }
}
