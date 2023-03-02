using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KOTE.UI.Armory
{
    public class SelectableGearItem : MonoBehaviour
    {
        public Image gearImage;
        public TMP_Text encumbranceText;

        public void Populate(int encumbrance)
        {
            encumbranceText.text = encumbrance.ToString();
        }
    }

    internal class GearItemData
    {
        public int encumbrance;
    }
}
