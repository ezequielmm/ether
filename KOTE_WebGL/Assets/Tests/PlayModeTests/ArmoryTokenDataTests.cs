using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class ArmoryTokenDataTests : MonoBehaviour
    {
        private ArmoryTokenData _tokenData;

        private NftMetaData testMetaData = new NftMetaData
        {
            image_url = "test.com",
            token_id = "0000",
            traits = new[]
            {
                new Trait
                {
                    trait_type = "helmet",
                    value = "helmet"
                }
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
            NftImageManager.Instance.DestroyInstance();
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
            Assert.AreEqual(testMetaData.token_id, _tokenData.Id);
        }

        [Test]
        public void DoesCreatingATokenDataObjectSetTokenImage()
        {
            _tokenData = new ArmoryTokenData(testMetaData);
            Assert.IsNotNull(_tokenData.NftImage);
            Assert.IsInstanceOf<Sprite>(_tokenData.NftImage);
        }

        [Test]
        public void DoesCallingImageReceivedUpdateTokenImage()
        {
            Assert.IsNotNull(_tokenData.NftImage);
            GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.Invoke("0000", null);
            Assert.IsNull(_tokenData.NftImage);
        }

        [Test]
        public void DoesCallingImageReceivedFireImageReceivedEvent()
        {
            bool eventFired = false;
            _tokenData.tokenImageReceived.AddListener(() => { eventFired = true; });
            GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.Invoke("0000", null);
        }
    }
}