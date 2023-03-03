using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableNftManager : MonoBehaviour
{
    public NftItem internalPrefab;
    public Toggle toggle;
    public bool isSelected;

    public void Populate(Nft nft, UnityAction<bool> onToggle)
    {
        internalPrefab.Populate(nft);
        toggle.onValueChanged.AddListener(onToggle);
    }
    
    public void DetermineToggleColor()
    {
        if(isSelected) toggle.targetGraphic.color = Color.green;
        if(!isSelected) toggle.targetGraphic.color = Color.white;
    }
}
