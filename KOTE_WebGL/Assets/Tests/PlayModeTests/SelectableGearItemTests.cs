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
        public void DoesEncumbranceTextExist()
        {
            Assert.NotNull(_itemManager.encumbranceText);
        }

        [Test]
        [TestCase(3)]
        [TestCase(0)]
        [TestCase(6)]
        [TestCase(12)]
        public void DoesPopulateChangeEncumbranceText(int encumbrance)
        {
            _itemManager.Populate(encumbrance);
            Assert.AreEqual(encumbrance.ToString(), _itemManager.encumbranceText.text);
        }
    }
}