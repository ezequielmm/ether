using TMPro;
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

        [SerializeField] private GameObject stackImage;
        [SerializeField] private TextMeshProUGUI stackText;
        
        public void UpdateSelectableBasedOnTokenType(NftContract curTokenType)
        {
            switch (curTokenType)
            {
                case NftContract.Villager:
                case NftContract.NonTokenVillager:
                    SetInteractiveForVillager();
                    break;
                case NftContract.Knights:
                case NftContract.BlessedVillager:
                    isInteractable = true;
                    GearImage.color = Color.white;
                    break;
                default:
                    isInteractable = false;
                    GearImage.color = Color.gray;
                    break;
            }
        }

        private void SetInteractiveForVillager()
        {
            if (ItemData.rarity != GearRarity.Common && ItemData.rarity != GearRarity.Uncommon)
            {
                isInteractable = false;
                GearImage.color = Color.gray;
            }
            else
            {
                isInteractable = true;
                GearImage.color = Color.white;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            // TODO: Abstract out more
            if (isInteractable) FindObjectOfType<ArmoryPanelManager>().OnGearItemSelected(ItemData);
        }

        public void Populate(GearItemData gearItemItem, int gearItemAmount)
        {
            base.Populate(gearItemItem);
            stackImage.SetActive(gearItemAmount > 1);
            
            var stackNumberText = gearItemAmount > 999 ? "999+" : gearItemAmount.ToString();
            stackText.text = stackNumberText;
        }
    }
}