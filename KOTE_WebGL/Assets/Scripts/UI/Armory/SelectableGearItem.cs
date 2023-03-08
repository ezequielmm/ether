using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class SelectableGearItem : MonoBehaviour
    {
        public Image gearImage;
        private GearItemData _itemData;
        public string Category => _itemData.category;
        public string ItemName => _itemData.name;
        public Sprite Image => _itemData.gearImage;
        public string Trait => _itemData.trait;

        internal void Populate(GearItemData newItemData)
        {
            _itemData = newItemData;
            if (newItemData.gearImage != null) gearImage.sprite = newItemData.gearImage;
        }

        public void OnItemClicked()
        {
            ArmoryPanelManager.OnGearSelected.Invoke(_itemData);
        }
    }

    public class GearItemData
    {
        public int gearId;
        public string name;
        public string trait;
        public string category;
        public Sprite gearImage;
    }
}
