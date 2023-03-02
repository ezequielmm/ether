using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManagerTests : MonoBehaviour
    {
        private NftData testNftList = new NftData
        {
            assets = new[]
            {
                new NftMetaData
                {
                    image_url = "nope",
                    token_id = "0000",
                    traits = new[]
                    {
                        new Trait
                        {
                            trait_type = "helmet",
                            value = "helmet"
                        }
                    }
                },
                new NftMetaData
                {
                    image_url = "nope",
                    token_id = "9999",
                    traits = new[]
                    {
                        new Trait
                        {
                            trait_type = "boots",
                            value = "boots"
                        }
                    }
                }
            }
        };

        private GearData testData = new GearData
        {
            gear = new List<GearItemData>
            {
                new GearItemData
                {
                    category = "Helmet",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Helmet"
                },
                new GearItemData
                {
                    category = "Pauldrons",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Pauldrons"
                },
                new GearItemData
                {
                    category = "Breastplate",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Breastplate"
                },
                new GearItemData
                {
                    category = "Legguards",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Legguards"
                },
                new GearItemData
                {
                    category = "Boots",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Boots"
                },
                new GearItemData
                {
                    category = "Weapon",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Weapon"
                },
                new GearItemData
                {
                    category = "Shield",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Shield"
                },
                new GearItemData
                {
                    category = "Padding",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Padding"
                },
                new GearItemData
                {
                    category = "Vambraces",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Vambraces"
                },
                new GearItemData
                {
                    category = "Gauntlet",
                    gearId = 1,
                    gearImage = null,
                    name = "Test",
                    trait = "Gauntlet"
                },
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

            GameObject armoryPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Armory/ArmoryPanel.prefab");
            armoryPanel = Instantiate(armoryPrefab);
            _armoryPanelManager = armoryPanel.GetComponent<ArmoryPanelManager>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            NftImageManager.Instance.DestroyInstance();
            GameManager.Instance.DestroyInstance();
            Destroy(armoryPanel);
            _armoryPanelManager = null;
            yield return null;
        }

        [Test]
        public void DoesOnGearSelectedEventExist()
        {
            Assert.IsNotNull(ArmoryPanelManager.OnGearSelected);
        }

        [Test]
        public void DoesPanelContainerExist()
        {
            Assert.NotNull(_armoryPanelManager.panelContainer);
        }

        [Test]
        public void DoesPlayButtonExist()
        {
            Assert.NotNull(_armoryPanelManager.playButton);
        }

        [Test]
        public void DoesNftImageExist()
        {
            Assert.NotNull(_armoryPanelManager.nftImage);
        }

        [Test]
        public void DoesHeaderPrefabExist()
        {
            Assert.NotNull(_armoryPanelManager.headerPrefab);
        }

        [Test]
        public void IsHeaderPrefabAnArmoryHeader()
        {
            Assert.IsInstanceOf<ArmoryHeaderManager>(_armoryPanelManager.headerPrefab);
        }

        [Test]
        public void DoesGearListTransformExist()
        {
            Assert.NotNull(_armoryPanelManager.gearListTransform);
        }

        [Test]
        public void DoGearSlotsExist()
        {
            Assert.Greater(_armoryPanelManager.gearSlots.Length, 0);
        }

        [Test]
        public void AreThereCorrectNumberOfGearSlots()
        {
            Assert.AreEqual(10, _armoryPanelManager.gearSlots.Length);
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
        public void DoesCallingNftMetadataReceivedPopulateCharacterImage()
        {
            _armoryPanelManager.nftImage.sprite = null;
            Assert.IsNull(_armoryPanelManager.nftImage.sprite);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            Assert.IsNotNull(_armoryPanelManager.nftImage.sprite);
        }

        [Test]
        public void DoesCallingNftMetadataClearList()
        {
            _armoryPanelManager.nftImage.sprite = null;
            Assert.IsNull(_armoryPanelManager.nftImage.sprite);
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            Assert.IsNotNull(_armoryPanelManager.nftImage.sprite);
            Assert.AreEqual(NftImageManager.Instance.defaultImage, _armoryPanelManager.nftImage.sprite);
        }

        [Test]
        public void DoesCallingOnNextTokenSwitchToNextToken()
        {
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.nftImage.sprite = null;
            Assert.IsNull(_armoryPanelManager.nftImage.sprite);
            _armoryPanelManager.OnNextToken();
            Assert.IsNotNull(_armoryPanelManager.nftImage.sprite);
        }

        [Test]
        public void DoesCallingOnPreviousTokenSwitchToPreviousToken()
        {
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnNextToken();
            _armoryPanelManager.nftImage.sprite = null;
            Assert.IsNull(_armoryPanelManager.nftImage.sprite);
            _armoryPanelManager.OnPreviousToken();
            Assert.IsNotNull(_armoryPanelManager.nftImage.sprite);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonSfxEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            _armoryPanelManager.OnPreviousToken();
            Assert.True(eventFired);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonCorrectSoundEffect()
        {
            string effectName = "";
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            _armoryPanelManager.OnPreviousToken();
            Assert.AreEqual("Button Click", effectName);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonRequestCorrectCategoryOfSfx()
        {
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            _armoryPanelManager.OnPreviousToken();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnNextTokenCallButtonSfxEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnNextToken();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnNextTokenCallButtonCorrectSoundEffect()
        {
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnNextToken();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnNextTokenCallButtonRequestCorrectCategoryOfSfx()
        {
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnNextToken();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonSfxEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnPlayButton();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonCorrectSoundEffect()
        {
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnPlayButton();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonRequestCorrectCategoryOfSfx()
        {
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnPlayButton();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnBackButtonCallButtonSfxEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnBackButton();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnBackButtonCallButtonCorrectSoundEffect()
        {
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnBackButton();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnBackButtonCallButtonRequestCorrectCategoryOfSfx()
        {
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnBackButton();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnPlayButtonCallNftSelected()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_NFT_SELECTED.AddListener((data) => { eventFired = true; });
            GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.Invoke(testNftList);
            _armoryPanelManager.OnPlayButton();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnBackButtonHideArmoryPanel()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            Assert.IsTrue(_armoryPanelManager.panelContainer.activeSelf);
            _armoryPanelManager.OnBackButton();
            Assert.IsFalse(_armoryPanelManager.panelContainer.activeSelf);
        }

        [Test]
        public void DoesExpeditionConfirmationCallMusicEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_MUSIC.AddListener((data, data2) => { eventFired = true; });
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesExpeditionConfirmationCallMusicEventTwice()
        {
            int timesFired = 0;
            GameManager.Instance.EVENT_PLAY_MUSIC.AddListener((data, data2) => { timesFired++; });
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
            Assert.AreEqual(2, timesFired);
        }

        [Test]
        public void DoesExpeditionConfirmationCallForMusic()
        {
            bool correctType = false;
            GameManager.Instance.EVENT_PLAY_MUSIC.AddListener((data, data2) =>
            {
                if (data == MusicTypes.Music) correctType = true;
            });
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
            Assert.True(correctType);
        }

        [Test]
        public void DoesExpeditionConfirmationCallForAmbience()
        {
            bool correctType = false;
            GameManager.Instance.EVENT_PLAY_MUSIC.AddListener((data, data2) =>
            {
                if (data == MusicTypes.Ambient) correctType = true;
            });
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
            Assert.True(correctType);
        }

        [UnityTest]
        public IEnumerator DoesCallingExpeditionConfirmedChangeScene()
        {
            GameManager.Instance.EVENT_EXPEDITION_CONFIRMED.Invoke();
            yield return new WaitForSeconds(0.5f);
            Assert.True(SceneManager.GetActiveScene().name == "Loader" ||
                        SceneManager.GetActiveScene().name == "Expedition");
        }

        [Test]
        public void DoesCallingGearReceivedCreateHeader()
        {
            GameManager.Instance.EVENT_GEAR_RECEIVED.Invoke(JsonConvert.SerializeObject(testData));
            ArmoryHeaderManager child =
                _armoryPanelManager.gearListTransform.GetComponentInChildren<ArmoryHeaderManager>();
            Assert.NotNull(child);
        }

        [Test]
        public void DoesCallingGearReceivedCreateCorrectNumberOfHeaders()
        {
            GameManager.Instance.EVENT_GEAR_RECEIVED.Invoke(JsonConvert.SerializeObject(testData));
            ArmoryHeaderManager[] children =
                _armoryPanelManager.gearListTransform.GetComponentsInChildren<ArmoryHeaderManager>();
            Assert.AreEqual(10, children.Length);
        }

        [Test]
        public void DoesCallingGearSelectedChangeSlotImage()
        {
            ArmoryPanelManager.OnGearSelected.Invoke(testData.gear[0]);
            Assert.IsNull(_armoryPanelManager.gearSlots[(int)GearCategories.Helmet].sprite);
        }
    }
}