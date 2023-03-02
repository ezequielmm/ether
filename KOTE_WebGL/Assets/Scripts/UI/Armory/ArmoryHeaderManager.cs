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

        public void Start()
        {
            // set the the dropdown to the 'default' values
            gearList.SetActive(false);
            dropdownArrow.sprite = arrowOptions[(int)ArrowDirections.Collapsed];
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(OnToggle);
        }

        public void Populate(string headerName)
        {
            title.text = headerName;
        }

        private void OnToggle(bool isOn)
        {
            gearList.SetActive(isOn);
        }

        private enum ArrowDirections
        {
            Collapsed,
            Expanded
        }
    }
}