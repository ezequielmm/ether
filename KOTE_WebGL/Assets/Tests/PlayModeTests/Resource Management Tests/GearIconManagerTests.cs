using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class GearIconManagerTests : MonoBehaviour
    {
        
        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject spriteManagerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/GearIconManager.prefab");
            GameObject nftSpriteManager = Instantiate(spriteManagerPrefab);
            nftSpriteManager.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameManager.Instance.DestroyInstance();
            GearIconManager.Instance.DestroyInstance();
            yield return null;
        }
        [Test]
        public void DoesDefaultImageExist()
        {
            Assert.NotNull(GearIconManager.Instance.defaultImage);
        }
    }
}