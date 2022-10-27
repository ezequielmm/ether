using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour
{
    public Image trinketImage;
    public TooltipAtCursor tooltipController;
    private string name;
    private string rarity;
    private string description;
    private Tooltip _tooltip;

    // uses string values for right now, will probably need to change once we parse trinket data
    public void Populate(Trinket trinket)
    {
        //TODO determine how to populate the trinket data
        name = trinket.name;
        rarity = trinket.rarity;
        description = trinket.description;
        trinketImage.sprite = SpriteAssetManager.Instance.GetTrinketImage(trinket.trinketId);
        _tooltip = new Tooltip()
        {
            title = name,
            description = description
        };
        tooltipController.SetTooltips(new List<Tooltip> { _tooltip });

    }
}