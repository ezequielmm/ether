using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableNftManager : MonoBehaviour
{
    public TreasuryNftItem internalPrefab;
    public Toggle toggle;
    public bool isSelected;

    public void Populate(NftMetaData nftMetaData, UnityAction<bool> onToggle)
    {
        internalPrefab.Populate(nftMetaData);
        toggle.onValueChanged.AddListener(onToggle);
    }
    
    public void DetermineToggleColor()
    {
        if(isSelected) toggle.targetGraphic.color = Color.green;
        if(!isSelected) toggle.targetGraphic.color = Color.white;
    }
}
