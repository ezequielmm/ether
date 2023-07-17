using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class ArmoryHeaderManager : MonoBehaviour
    {
        public GameObject gearList;
        public Image dropdownArrow;
        public Sprite[] arrowOptions;
        public Toggle toggle;
        public TMP_Text title;
        public SelectableGearItem gearPrefab;

        private List<SelectableGearItem> gearItems = new();

        public void Start()
        {
            // set the the dropdown to the 'default' values
            gearList.SetActive(false);
            dropdownArrow.sprite = arrowOptions[(int)ArrowDirections.Collapsed];
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(OnToggle);
        }

        internal void Populate(string headerName, List<GearItemData> gearData)
        {
            title.text = headerName;
            GenerateGearItems(gearData);
        }
        
        public void Populate(string currentCategoryName, List<CategoryItem> gearData)
        {
            title.text = currentCategoryName;
            foreach (var gearItem in gearData)
            {
                var item = Instantiate(gearPrefab, gearList.transform);
                item.Populate(gearItem.item, gearItem.amount);
                gearItems.Add(item);
            }
        }

        private void GenerateGearItems(List<GearItemData> gearData)
        {
            foreach (GearItemData gearItem in gearData)
            {
                SelectableGearItem item = Instantiate(gearPrefab, gearList.transform);
                item.Populate(gearItem);
                gearItems.Add(item);
            }
        }

        internal void UpdateGearSelectableStatus(NftContract tokenType)
        {
            foreach (SelectableGearItem item in gearItems)
            {
                item.UpdateSelectableBasedOnTokenType(tokenType);
            }
        }

        public void OnToggle(bool isOn)
        {
            gearList.SetActive(isOn);
        }

        private enum ArrowDirections
        {
            Collapsed,
            Expanded
        }

        public void ClearList()
        {
            foreach (SelectableGearItem item in gearItems)
            {
                Destroy(item.gameObject);
            }
            gearItems.Clear();
        }
    }
}