using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
public class CampHealAnimationManagerTests : MonoBehaviour
{
    private CampHealAnimationManager campHealManager;
    private ParticleSystem healEffect;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject CampHealEffectPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/CampHealEffect.prefab");
        GameObject CampHealEffectInstance = Instantiate(CampHealEffectPrefab);
        campHealManager = CampHealEffectInstance.GetComponent<CampHealAnimationManager>();
        healEffect = CampHealEffectInstance.GetComponentInChildren<ParticleSystem>();
        CampHealEffectInstance.SetActive(true);
        yield return null;
    }

    [Test]
    public void DoesOnHealStartHealEffect()
    {
        GameManager.Instance.EVENT_HEAL.Invoke("camp", 1);
        Assert.True(healEffect.isPlaying);
    }
}
