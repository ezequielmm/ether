using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class GearSlot : MonoBehaviour
    {
        [SerializeField] internal GearCategories gearCategory;
        public Trait gearTrait;
        public Image icon;
        private GearItemData selectedGear;
        internal GearItemData defaultGear;

        internal GearItemData GetEquippedGear()
        {
            if (selectedGear == defaultGear) return null;
            return selectedGear;
        }

        internal void SetGearInSlot(GearItemData currentGear)
        {
            selectedGear = currentGear;
            icon.sprite =currentGear.gearImage;
        }

        internal void ResetSlot()
        {
            if (defaultGear != null)
            {
                SetGearInSlot(defaultGear);
                return;
            }
            icon.sprite = GearIconManager.Instance.defaultImage;
            selectedGear = null;
        }
    }
}