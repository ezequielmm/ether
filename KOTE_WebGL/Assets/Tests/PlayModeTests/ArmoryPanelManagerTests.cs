using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class ArmoryPanelManagerTests : MonoBehaviour
    {
        private List<Nft> testNftList;

        private GearData testData = new GearData
        {
            ownedGear = new List<GearItemData>
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
                    trait = "Legguard"
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
                    trait = "Vambrace"
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

        private GameObject armoryPanel;
        private GameObject nftSpriteManager;
        private ArmoryPanelManager _armoryPanelManager;
        private Sprite testSprite;


        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject spriteManagerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/PlayerSpriteManager.prefab");
            nftSpriteManager = Instantiate(spriteManagerPrefab);
            nftSpriteManager.SetActive(true);

            GameObject armoryPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Armory/ArmoryPanel.prefab");
            armoryPanel = Instantiate(armoryPrefab);
            _armoryPanelManager = armoryPanel.GetComponent<ArmoryPanelManager>();
            testSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Main_Menu/CharSelection/knight.png");

            testNftList = new()
            {
                new Nft()
                {
                    ImageUrl = "test.com",
                    TokenId = 0,
                    Traits = new Dictionary<Trait, string>()
                    {
                        { Trait.Helmet, "helmet" }
                    },
                    Contract = NftContract.Knights
                },
                new Nft()
                {
                    ImageUrl = "nope",
                    TokenId = 9999,
                    Traits = new Dictionary<Trait, string>()
                    {
                        { Trait.Boots, "boots" }
                    },
                    Contract = NftContract.Knights
                }
            };

            foreach (Nft nft in testNftList)
            {
                nft.Image = testSprite;
            }

            _armoryPanelManager.defaultCharacterSprite = testSprite;
            NftManager.Instance.Nfts = new Dictionary<NftContract, List<Nft>>();
            NftManager.Instance.Nfts[NftContract.Knights] = testNftList;
            NftManager.Instance.NftsLoaded.Invoke();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Destroy(nftSpriteManager);
            Destroy(armoryPanel);
            _armoryPanelManager = null;
            GameManager.Instance.DestroyInstance();
            GearIconManager.Instance.DestroyInstance();
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
        public void DoesPortraitManagerExist()
        {
            Assert.NotNull(_armoryPanelManager.portraitManager);
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
            Assert.Greater(_armoryPanelManager.gearSlots.Count, 0);
        }

        [Test]
        public void AreThereCorrectNumberOfGearSlots()
        {
            Assert.AreEqual(10, _armoryPanelManager.gearSlots.Count);
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
        public void DoesShowingPanelCallNftSelectedEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_NFT_SELECTED.AddListener((data) => { eventFired = true; });
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            _armoryPanelManager.OnNextToken();
            Assert.True(eventFired);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonSfxEvent()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            bool eventFired = false;
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            _armoryPanelManager.OnPreviousToken();
            Assert.True(eventFired);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallNftSelectedEvent()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            bool eventFired = false;
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_NFT_SELECTED.AddListener((data) => { eventFired = true; });
            _armoryPanelManager.OnPreviousToken();
            Assert.True(eventFired);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonCorrectSoundEffect()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            string effectName = "";
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            _armoryPanelManager.OnPreviousToken();
            Assert.AreEqual("Button Click", effectName);
        }

        [UnityTest]
        public IEnumerator DoesOnPreviousTokenCallButtonRequestCorrectCategoryOfSfx()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            yield return null;
            _armoryPanelManager.OnNextToken();
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            _armoryPanelManager.OnPreviousToken();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnNextTokenCallButtonSfxEvent()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            _armoryPanelManager.OnNextToken();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnNextTokenCallNftSelectedEvent()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            bool eventFired = false;
            GameManager.Instance.EVENT_NFT_SELECTED.AddListener((data) => { eventFired = true; });
            _armoryPanelManager.OnNextToken();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnNextTokenCallButtonCorrectSoundEffect()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            _armoryPanelManager.OnNextToken();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnNextTokenCallButtonRequestCorrectCategoryOfSfx()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            _armoryPanelManager.OnNextToken();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonSfxEvent()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            _armoryPanelManager.OnPlayButton();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonCorrectSoundEffect()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            _armoryPanelManager.OnPlayButton();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnPlayButtonCallButtonRequestCorrectCategoryOfSfx()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            _armoryPanelManager.OnPlayButton();
            Assert.AreEqual(SoundTypes.UI, requestedType);
        }

        [Test]
        public void DoesOnBackButtonCallButtonSfxEvent()
        {
            bool eventFired = false;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
            _armoryPanelManager.OnBackButton();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnBackButtonCallButtonCorrectSoundEffect()
        {
            string effectName = "";
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, sfxName) => { effectName = sfxName; });
            _armoryPanelManager.OnBackButton();
            Assert.AreEqual("Button Click", effectName);
        }

        [Test]
        public void DoesOnBackButtonCallButtonRequestCorrectCategoryOfSfx()
        {
            SoundTypes requestedType = SoundTypes.EnemyOffensive;
            GameManager.Instance.EVENT_PLAY_SFX.AddListener((soundType, data2) => { requestedType = soundType; });
            _armoryPanelManager.OnBackButton();
            Assert.AreEqual(SoundTypes.UI, requestedType);
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
        public void DoesCallingGearSelectedNotChangeSlotImageIfKnight()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            _armoryPanelManager.gearSlots[0].icon.sprite = null;

            ArmoryPanelManager.OnGearSelected.Invoke(testData.ownedGear[0]);
            Assert.IsNull(_armoryPanelManager.gearSlots[0].icon.sprite);
        }

        [Test]
        public void DoesCallingGearSelectedChangeSlotImageIfNotKnight()
        {
            testNftList[0].Contract = NftContract.Villager;
            testNftList[0].Traits = new Dictionary<Trait, string>
            {
                { Trait.Helmet, "Basic Bucket Helmet" },
                { Trait.Padding, "Red" },
                { Trait.Shield, "Rusty Shield" },
                { Trait.Weapon, "Rusty Sword" }
            };
            NftManager.Instance.Nfts.Clear();
            NftManager.Instance.Nfts[NftContract.Villager] = testNftList;
            NftManager.Instance.NftsLoaded.Invoke();

            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            ArmoryPanelManager.OnGearSelected.Invoke(testData.ownedGear[0]);
            Assert.IsNull(_armoryPanelManager.gearSlots[0].icon.sprite);
        }

        [Test]
        public void DoKnightNftsDeactivateGearPanels()
        {
            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            foreach (GameObject panel in _armoryPanelManager.gearPanels)
            {
                Assert.False(panel.activeSelf);
            }
        }

        [Test]
        public void DoesLoadingAVillagerUpdateGearSlots()
        {
            testNftList[0].Contract = NftContract.Villager;
            testNftList[0].Traits = new Dictionary<Trait, string>
            {
                { Trait.Helmet, "Basic Bucket Helmet" },
                { Trait.Padding, "Red" },
                { Trait.Shield, "Rusty Shield" },
                { Trait.Weapon, "Rusty Sword" }
            };
            NftManager.Instance.Nfts.Clear();
            NftManager.Instance.Nfts[NftContract.Villager] = testNftList;
            NftManager.Instance.NftsLoaded.Invoke();

            GameManager.Instance.EVENT_SHOW_ARMORY_PANEL.Invoke(true);
            foreach (GearSlot slot in _armoryPanelManager.gearSlots)
            {
                Assert.IsNull(slot.GetEquippedGear());
            }
        }

        [Test]
        public void DoesSendingEmptyNftListDeactivatePlayButton()
        {
            NftManager.Instance.Nfts.Clear();
            NftManager.Instance.NftsLoaded.Invoke();
            Assert.False(_armoryPanelManager.playButton.interactable);
        }

        [Test]
        public void DoesPopulatingGearCreateHeadersWithNoItems()
        {
            FetchData.Instance.TestData[FetchType.GearInventory] = JsonConvert.SerializeObject(new TestGearData{data = testData});
            GameManager.Instance.EVENT_AUTHENTICATED.Invoke();
            ArmoryHeaderManager[] headers =
                _armoryPanelManager.gearListTransform.GetComponentsInChildren<ArmoryHeaderManager>();
            Assert.AreEqual(10, headers.Length);
            foreach (ArmoryHeaderManager header in headers)
            {
                Assert.AreEqual(0, header.gameObject.GetComponentsInChildren<SelectableGearItem>().Length);
            }
        }
    }

    public class TestGearData
    {
        public GearData data;
    }
}