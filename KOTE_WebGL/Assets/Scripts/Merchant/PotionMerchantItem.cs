using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionMerchantItem : MerchantItem<MerchantData.Merchant<PotionData>>
{
    [SerializeField] PotionManager potionManager;

    public override void Populate(MerchantData.Merchant<PotionData> data)
    {
        potionManager.enabled = true;
        base.Populate(data);
        potionManager.Populate(data.item);
        TooltipAtCursor tooltipManager = potionManager.GetComponent<TooltipAtCursor>();
        tooltipManager.limit = -Vector2.one;
        tooltipManager.anchor = TooltipController.Anchor.TopLeft;
        potionManager.enabled = false;
    }
}