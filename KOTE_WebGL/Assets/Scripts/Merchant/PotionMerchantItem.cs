using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionMerchantItem : MerchantItem<MerchantData.Merchant<PotionData>>
{
    public override void Populate(MerchantData.Merchant<PotionData> data)
    {
        base.Populate(data);
    }
}
