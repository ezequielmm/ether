using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class SelectableGearItem : GearItem, IPointerDownHandler
    {
        public string Category => ItemData.category;
        public string ItemName => ItemData.name;
        public Sprite Image => ItemData.gearImage;
        public string Trait => ItemData.trait;

        public void OnPointerDown(PointerEventData data)
        {
            // TODO: Abstract out more
            ArmoryPanelManager.OnGearSelected.Invoke(ItemData);
        }
    }
}