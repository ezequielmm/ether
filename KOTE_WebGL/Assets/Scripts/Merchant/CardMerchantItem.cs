using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMerchantItem : MerchantItem<MerchantData.Merchant<Card>>
{
    [SerializeField]
    private UICardPrefabManager card;
    public override void Populate(MerchantData.Merchant<Card> data)
    {
        card.Populate(data.item);
        base.Populate(data);
    }
}
