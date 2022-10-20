using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using static CardOnHandManager;

public class UICardPrefabManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI energyTF;
    public TextMeshProUGUI nameTF;
    public TextMeshProUGUI rarityTF;
    public TextMeshProUGUI descriptionTF;

    public Image gemSprite;
    public Image frameSprite;
    public Image bannerSprite;
    public Image cardImage;

    public CardOnHandManager managerReference;
    public string id;

    private Vector3 originalScale;
    public bool scaleCardOnHover = true;
    public float scaleOnHover = 2;
    

    private Card card;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void populate(Card card)
    {
        id = card.id;
        string cardEnergy = Mathf.Max(card.energy, 0).ToString();
        if (card.energy < 0)
        {
            cardEnergy = "X";
        }
        energyTF.SetText(cardEnergy);
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);

        string cardType = card.cardType;
        CardAssetManager cardAssetManager = CardAssetManager.Instance;
        gemSprite.sprite = cardAssetManager.GetGem(card.cardType);
        frameSprite.sprite = cardAssetManager.GetFrame(card.pool);
        bannerSprite.sprite = cardAssetManager.GetBanner(card.rarity);
        cardImage.sprite = cardAssetManager.GetCardImage(card.cardId);
      
        this.card = card;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCardOnHover == false) return;
        DOTween.Kill(this.transform);
        transform.DOScale(scaleOnHover, 0.3f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCardOnHover == false) return;
        DOTween.Kill(this.transform);
        transform.DOScale(originalScale, 0.3f);
    }
}
