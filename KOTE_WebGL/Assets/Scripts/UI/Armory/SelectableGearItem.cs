using UnityEngine;
using UnityEngine.EventSystems;

namespace KOTE.UI.Armory
{
    public class SelectableGearItem : GearItem, IPointerDownHandler
    {
        public string Category => ItemData.category;
        public string ItemName => ItemData.name;
        public Sprite Image => ItemData.gearImage;
        public string Trait => ItemData.trait;

        private bool isInteractable = true;

        public void UpdateSelectableBasedOnTokenType(NftContract curTokenType)
        {
            switch (curTokenType)
            {
                case NftContract.Villager:
                    SetInteractiveForVillager();
                    break;
                case NftContract.Knights:
                    isInteractable = false;
                    break;
                case NftContract.BlessedVillager:
                    isInteractable = true;
                    break;
                default:
                    isInteractable = false;
                    break;
            }
        }

        private void SetInteractiveForVillager()
        {
            if (ItemData.rarity != GearRarity.Common && ItemData.rarity != GearRarity.Uncommon)
            {
                isInteractable = false;
            }
            else
            {
                isInteractable = true;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            // TODO: Abstract out more
            if (isInteractable) ArmoryPanelManager.OnGearSelected.Invoke(ItemData);
        }
    }
}