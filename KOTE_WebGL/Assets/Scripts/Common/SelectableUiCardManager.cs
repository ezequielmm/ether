using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableUiCardManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Toggle cardSelectorToggle;
    // keep track of this manually so we can have multiple cards selected 
    public bool isSelected;
    [SerializeField]private UICardPrefabManager uiCardManager;

    public void Populate(Card card)
    {
        uiCardManager.Populate(card);
    }

    public string GetId()
    {
        return uiCardManager.id;
    }

    public void DetermineToggleColor()
    {
        if(isSelected) cardSelectorToggle.targetGraphic.color = Color.green;
        if(!isSelected) cardSelectorToggle.targetGraphic.color = Color.clear;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        uiCardManager.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       uiCardManager.OnPointerExit(eventData);
    }
}
