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

        internal void SetGearInSlot(GearItemData currentGear)
        {
            selectedGear = currentGear;
            icon.sprite =currentGear.gearImage;
        }
    }
}