using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour
{
    public Image trinketImage;
    public TooltipAtCursor tooltipController;

    public string Id => _trinket.id;
    private Trinket _trinket;
    private Tooltip _tooltip;

    // uses string values for right now, will probably need to change once we parse trinket data
    public void Populate(Trinket trinket)
    {
        _trinket = trinket;
        trinketImage.sprite = SpriteAssetManager.Instance.GetTrinketImage(trinket.trinketId);
        _tooltip = new Tooltip()
        {
            title = _trinket.name,
            description = _trinket.description
        };
        tooltipController.SetTooltips(new List<Tooltip> { _tooltip });
    }
}