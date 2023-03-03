using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class ArmoryTokenDataTests : MonoBehaviour
    {
        private ArmoryTokenData _tokenData;

        private Nft testMetaData = new Nft()
        {
            ImageUrl = "test.com",
            TokenId = 0,
            Traits = new Dictionary<Trait, string>()
            {
                { Trait.Helmet, "helmet" }
            }
        };

        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject spriteManagerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/NftSpriteManager.prefab");
           GameObject nftSpriteManager = Instantiate(spriteManagerPrefab);
            nftSpriteManager.SetActive(true);
            _tokenData = new ArmoryTokenData(testMetaData);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameManager.Instance.DestroyInstance();
            yield return null;
        }

        [Test]
        public void DoesCallingConstructorCreateArmoryTokenDataObject()
        {
            ArmoryTokenData tokenData = null;
            Assert.IsNull(tokenData);
            tokenData = new ArmoryTokenData(testMetaData);
            Assert.IsInstanceOf<ArmoryTokenData>(tokenData);
        }

        [Test]
        public void DoesCreatingATokenDataObjectPopulateMetadata()
        {
            _tokenData = new ArmoryTokenData(testMetaData);
            Assert.IsNotNull(_tokenData.MetaData);
            Assert.AreEqual(testMetaData, _tokenData.MetaData);
        }

        [Test]
        public void DoesCreatingATokenDataObjectPopulateId()
        {
            _tokenData = new ArmoryTokenData(testMetaData);
            Assert.AreEqual(testMetaData.TokenId, _tokenData.Id);
        }
    }
}