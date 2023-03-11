using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class SelectableGearItem : GearItem
    {
        public string Category => ItemData.category;
        public string ItemName => ItemData.name;
        public Sprite Image => ItemData.gearImage;
        public string Trait => ItemData.trait;

        public void SendItemToArmoryPanel()
        {
            // TODO: Abstract out more
            ArmoryPanelManager.OnGearSelected.Invoke(ItemData);
        }
    }
}
