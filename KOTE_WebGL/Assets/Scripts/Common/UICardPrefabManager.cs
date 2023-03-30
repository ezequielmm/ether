using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICardPrefabManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TooltipAtCursor tooltipManager;

    public TextMeshProUGUI energyTF;
    public TextMeshProUGUI nameTF;
    public TextMeshProUGUI rarityTF;
    public TextMeshProUGUI descriptionTF;

    public Image gemSprite;
    public Image frameSprite;
    public Image bannerSprite;
    public Image cardImage;

    public string id => card?.id;

    private Vector3 originalScale;
    public bool scaleCardOnHover = true;
    public float scaleOnHover = 2;

    [SerializeField] private Sprite cardBackground;
    [SerializeField] private Sprite cardSelectedBackground;

    [SerializeField] Image cardBackgroundImage;

    public bool useBackgroundImage = false;

    private Card card;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Populate(Card card)
    {
        if (card == null)
        {
            Debug.LogError($"[UICardPrefabManager] Card can not be Null!");
            return;
        }

        string cardEnergy = Mathf.Max(card.energy, 0).ToString();
        if (card.energy < 0)
        {
            cardEnergy = "X";
        }

        energyTF.SetText(cardEnergy);
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);

        CardAssetManager cardAssetManager = CardAssetManager.Instance;
        gemSprite.sprite = cardAssetManager.GetGem(card.cardType, card.isUpgraded);
        // check to see if it's a curse or status card, because they have unique frames within the neutral pool
        if (card.cardType == "curse" || card.cardType == "status")
        {
            frameSprite.sprite = cardAssetManager.GetFrame(card.cardType);
        }
        else
        {
            frameSprite.sprite = cardAssetManager.GetFrame(card.pool);
        }

        bannerSprite.sprite = cardAssetManager.GetBanner(card.rarity);
        cardImage.sprite = cardAssetManager.GetCardImage(card.cardId);

        this.card = card;
        PopulateToolTips();

        Deselect();
    }

    private void PopulateToolTips()
    {
        List<Tooltip> tooltips = new();
        if (card.properties.statuses != null && card.properties.statuses.Count != 0)
        {
            foreach (var status in card.properties.statuses)
            {
                if (!string.IsNullOrEmpty(status.tooltip.title))
                {
                    tooltips.Add(status.tooltip);
                }
                else
                {
                    var description = status.args.description ?? "TODO // Add Description";
                    tooltips.Add(new Tooltip()
                    {
                        title = Utils.PrettyText(status.name),
                        description = description
                    });
                }
            }
        }

        tooltipManager.SetTooltips(tooltips);
    }

    public void Select()
    {
        if (useBackgroundImage)
        {
            cardBackgroundImage.enabled = true;
            cardBackgroundImage.sprite = cardSelectedBackground;
        }
    }

    public void Deselect()
    {
        if (useBackgroundImage)
        {
            cardBackgroundImage.enabled = true;
            cardBackgroundImage.sprite = cardBackground;
        }
        else
        {
            cardBackgroundImage.sprite = null;
            cardBackgroundImage.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipManager.OnPointerEnter(eventData);
        if (scaleCardOnHover == false) return;
        DOTween.Kill(this.transform);
        transform.DOScale(scaleOnHover, 0.3f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipManager.OnPointerExit(eventData);
        if (scaleCardOnHover == false) return;
        DOTween.Kill(this.transform);
        transform.DOScale(originalScale, 0.3f);
    }
}