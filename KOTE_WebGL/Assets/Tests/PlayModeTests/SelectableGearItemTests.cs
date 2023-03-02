using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class SelectableGearItemTests : MonoBehaviour
    {
        private GameObject selectableGearItem;
        private SelectableGearItem _itemManager;

        private GearItemData testItemData = new GearItemData
        {
            gearId = 1,
            name = "Test",
            trait = "Helmet",
            category = "Helmet",
            gearImage = null
        };

        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject GearItemPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Armory/SelectableGear.prefab");
            selectableGearItem = Instantiate(GearItemPrefab);
            _itemManager = selectableGearItem.GetComponent<SelectableGearItem>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Destroy(selectableGearItem);
            _itemManager = null;
            yield return null;
        }

        [Test]
        public void DoesGearImageExist()
        {
            Assert.NotNull(_itemManager.gearImage);
        }

        [Test]
        public void DoesPopulatingItemSetName()
        {
            _itemManager.Populate(testItemData);
            Assert.AreEqual("Test", _itemManager.ItemName);
        }

        [Test]
        public void DoesPopulatingItemSetCategory()
        {
            _itemManager.Populate(testItemData);
            Assert.AreEqual("Helmet", _itemManager.Category);
        }

        [Test]
        public void DoesPopulatingItemSetTrait()
        {
            _itemManager.Populate(testItemData);
            Assert.AreEqual("Helmet", _itemManager.Trait);
        }

        [Test]
        public void DoesPopulatingItemSetItemImage()
        {
            _itemManager.Populate(testItemData);
            Assert.IsNull(_itemManager.Image);
        }

        [Test]
        public void DoesOnItemClickedCallOnGearSelected()
        {
            bool eventFired = false;
            ArmoryPanelManager.OnGearSelected.AddListener((data) => { eventFired = true; });
            _itemManager.Populate(testItemData);
            _itemManager.OnItemClicked();
            Assert.True(eventFired);
        }

        [Test]
        public void DoesOnItemClickedSendCorrectData()
        {
            GearItemData receivedData = null;
            ArmoryPanelManager.OnGearSelected.AddListener((data) => { receivedData = data; });
            _itemManager.Populate(testItemData);
            _itemManager.OnItemClicked();
            Assert.AreEqual(testItemData, receivedData);
        }
    }
}