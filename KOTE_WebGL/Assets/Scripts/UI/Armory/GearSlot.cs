using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class GearSlot : MonoBehaviour, IPointerDownHandler
    {
        public TooltipAtCursor tooltip;
        public Trait gearTrait;
        public Image icon;
        private GearItemData selectedGear;
        
        public void OnPointerDown(PointerEventData data)
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
            tooltip.SetTooltips(new List<Tooltip>
            {
                new Tooltip
                {
                    title = $"{currentGear.name}{FormatTraitText()}"
                }
            });
            tooltip.enabled = true;
        }

        private string FormatTraitText()
        {
            if (selectedGear.trait == "Weapon") return "";
            return " " + selectedGear.trait;
        }

        internal void ResetSlot()
        {
            icon.sprite = GearIconManager.Instance.defaultImage;
            tooltip.enabled = false;
            selectedGear = null;
        }
    }
}