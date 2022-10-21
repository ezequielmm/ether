using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinketMerchantItem : MerchantItem<MerchantData.Merchant<Trinket>>
{
    [SerializeField]
    TrinketItemManager trinketManager;
    public override void Populate(MerchantData.Merchant<Trinket> data)
    {
        base.Populate(data);
        trinketManager.Populate(data.item);
        trinketManager.enabled = false;
    }
}
