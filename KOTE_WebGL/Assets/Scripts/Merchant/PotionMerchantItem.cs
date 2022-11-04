using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionMerchantItem : MerchantItem<MerchantData.Merchant<PotionData>>
{
    [SerializeField]
    PotionManager potionManager;
    public override void Populate(MerchantData.Merchant<PotionData> data)
    {
        potionManager.enabled = true;
        base.Populate(data);
        potionManager.Populate(data.item);
        potionManager.GetComponent<TooltipAtCursor>().enabled = false;
        potionManager.enabled = false;
    }
}
