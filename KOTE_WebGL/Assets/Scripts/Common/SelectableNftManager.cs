using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableNftManager : MonoBehaviour
{
    public TreasuryNftItem internalPrefab;
    public Toggle toggle;

    public void Populate(NftMetaData nftMetaData, UnityAction<bool> onToggle)
    {
        internalPrefab.Populate(nftMetaData);
        toggle.onValueChanged.AddListener(onToggle);
    }
}
