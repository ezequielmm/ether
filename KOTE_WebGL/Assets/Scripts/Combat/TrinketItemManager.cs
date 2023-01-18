using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour, IPointerClickHandler
{
    public Image trinketImage;
    public GameObject counter;
    public TMP_Text counterText;
    public TooltipAtCursor tooltipController;
    public Image selectFrame;

    public string Id => _trinket.id;
    private Trinket _trinket;
    private Tooltip _tooltip;
    
    // select variables
    private bool enablePointerClick;
    private Action<bool> onTrinketSelected;
    public bool isToggled;

    private void Start()
    {
        selectFrame.gameObject.SetActive(false);
    }

    // uses string values for right now, will probably need to change once we parse trinket data
    // selectMode and onSelected are used to allow us to select trinkets from the trinket panel
    public void Populate(Trinket trinket, bool selectMode = false, Action<bool> onSelected = null)
    {
        _trinket = trinket;
        trinketImage.sprite = SpriteAssetManager.Instance.GetTrinketImage(trinket.trinketId);
        _tooltip = new Tooltip()
        {
            title = _trinket.name,
            description = _trinket.description
        };
        tooltipController.SetTooltips(new List<Tooltip> { _tooltip });
        if (selectMode)
        {
            counter.SetActive(false);
            enablePointerClick = true;
            onTrinketSelected = onSelected;
        }
    }

    // utility function to update the toggle status from within the OnTrinketSelected action
    public void UpdateToggleStatus()
    {
        selectFrame.gameObject.SetActive(isToggled);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (enablePointerClick)
        {
            onTrinketSelected?.Invoke(!isToggled);
        }
    }
}