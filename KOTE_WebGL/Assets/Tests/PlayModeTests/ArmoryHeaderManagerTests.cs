using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace KOTE.UI.Armory
{
    public class ArmoryHeaderManagerTests : MonoBehaviour
    {
        private GameObject armoryHeaderObject;
        private ArmoryHeaderManager _headerManager;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject GearListHeaderPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Armory/GearListHeader.prefab");
            armoryHeaderObject = Instantiate(GearListHeaderPrefab);
            _headerManager = armoryHeaderObject.GetComponent<ArmoryHeaderManager>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Destroy(armoryHeaderObject);
            _headerManager = null;
            yield return null;
        }

        [Test]
        public void DoesGearListExist()
        {
            Assert.NotNull(_headerManager.gearList);
        }

        [Test]
        public void DoesDropdownArrowImageExist()
        {
            Assert.NotNull(_headerManager.dropdownArrow);
        }

        [Test]
        public void DoesArrowArrayExist()
        {
            Assert.NotNull(_headerManager.arrowOptions);
        }

        [Test]
        public void DoesArrowArrayHaveCorrectNumberOfImages()
        {
            Assert.AreEqual(2, _headerManager.arrowOptions.Length);
        }

        [Test]
        public void DoesToggleExist()
        {
            Assert.NotNull(_headerManager.toggle);
        }

        [Test]
        public void DoesTitleTextExist()
        {
            Assert.NotNull(_headerManager.title);
        }

        [Test]
        public void DoesSelectableGearItemPrefabExist()
        {
            Assert.NotNull(_headerManager.gearPrefab);
        }

        [Test]
        public void IsSelectableGearPrefabASelectableGearPiece()
        {
            Assert.IsInstanceOf<SelectableGearItem>(_headerManager.gearPrefab);
        }

        [Test]
        public void IsGearListDeactivatedOnStart()
        {
            Assert.False(_headerManager.gearList.activeSelf);
        }

        [Test]
        public void IsDropdownArrowCorrectDirectionOnStart()
        {
            Assert.AreEqual(_headerManager.arrowOptions[0], _headerManager.dropdownArrow.sprite);
        }

        [Test]
        public void IsToggleOffOnStart()
        {
            Assert.False(_headerManager.toggle.isOn);
        }

        [Test]
        public void DoesToggleActivateGearList()
        {
            _headerManager.toggle.isOn = true;
            Assert.True(_headerManager.gearList.activeSelf);
        }

        [Test]
        public void DoesPopulatingHeaderChangeHeaderName()
        {
            _headerManager.Populate("Helmet");
            Assert.AreEqual("Helmet", _headerManager.title.text);
        }
    }
}
