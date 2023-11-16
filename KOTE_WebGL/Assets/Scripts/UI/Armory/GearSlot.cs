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
        private VictoryItems selectedGear;
        public Sprite defaultImage;
        
        public void OnPointerDown(PointerEventData data)
        {
            ResetSlot();
            FindObjectOfType<ArmoryPanelManager>().OnGearItemRemoved(this, gearTrait);
        }

        internal VictoryItems GetEquippedGear()
        {
            return selectedGear;
        }

        internal void SetGearInSlot(VictoryItems currentGear, float alphaColor = 1f)
        {
            selectedGear = currentGear;
            icon.sprite = currentGear.gearImage;
            var color = icon.color;
            color.a = alphaColor;
            icon.color = color;
            
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
            icon.sprite = defaultImage;
            tooltip.enabled = false;
            selectedGear = null;
        }
    }
}