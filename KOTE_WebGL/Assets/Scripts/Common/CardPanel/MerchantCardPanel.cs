using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MerchantCardPanel : CardPanelBase
{
    public UnityEvent<string, UICardPrefabManager> OnCardClick = new UnityEvent<string, UICardPrefabManager>();

    protected override void Start()
    {
        base.Start();
    }

    public override UICardPrefabManager OnGenerateCard(UICardPrefabManager uiCard)
    {
        var button = uiCard.GetComponentInChildren<Button>(true);
        if (button != null)
        {
            button.enabled = true;
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() =>
            {
                OnCardClick.Invoke(uiCard.id, uiCard);
            });
        }
        return uiCard;
    }
}
