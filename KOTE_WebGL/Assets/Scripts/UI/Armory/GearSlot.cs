using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class GearSlot : MonoBehaviour
    {
        public Trait gearTrait;
        public Image icon;
        private GearItemData selectedGear;

        public void OnClick()
        {
            ResetSlot();
            ArmoryPanelManager.OnSlotCleared.Invoke(gearTrait);
        }

        internal GearItemData GetEquippedGear()
        {
            return selectedGear;
        }

        internal void SetGearInSlot(GearItemData currentGear)
        {
            selectedGear = currentGear;
            icon.sprite = currentGear.gearImage;
        }

        internal void ResetSlot()
        {
            icon.sprite = GearIconManager.Instance.defaultImage;
            selectedGear = null;
        }
    }
}