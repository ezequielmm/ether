using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PotionManager : MonoBehaviour, IPointerClickHandler
{
    public Sprite unusedPotionSprite, usedPotionSprite;
    public bool pointerActive;
    private Image potionImage;
    private Button potionButton;

    private TooltipAtCursor tooltipController;

    private HeldPotion potion;

    private Tooltip unknown;
    private Tooltip _tooltip;

    public TargetProfile targetProfile { get; private set; }

    private float SizeOnHover = 1.3f;
    private float originalY;

    private void Awake()
    {
        tooltipController = GetComponent<TooltipAtCursor>();
        potionImage = GetComponent<Image>();
        potionButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (pointerActive)
        {
            PointerData pointerData = new PointerData(gameObject.transform.position, PointerOrigin.potion,
                targetProfile);
            GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(pointerData);
            GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(false);
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.Instance.EVENT_DEACTIVATE_POINTER.Invoke(potion.id);
                GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(true);
                pointerActive = false;
            }
        }
    }

    public void Populate(HeldPotion inPotion)
    {
        if (inPotion == null)
        {
            PopulateAsEmpty();
            return;
        }

        potion = inPotion;
        PopulatePotion();
    }

    private void PopulateAsEmpty()
    {
        tooltipController.enabled = false;
        potionImage.sprite = unusedPotionSprite;
        targetProfile = new TargetProfile()
        {
            player = true,
            enemy = true
        };
        originalY = 0;
    }

    private void PopulatePotion()
    {
        tooltipController.enabled = true;
        _tooltip = new Tooltip()
        {
            title = potion.potion.name,
            description = potion.potion.description
        };

        potionImage.sprite = SpriteAssetManager.Instance.GetPotionImage(potion.potion.potionId);
        tooltipController.SetTooltips(new List<Tooltip> { _tooltip });
        targetProfile = new TargetProfile();
        foreach (Effect effect in potion.potion.effects)
        {
            if (effect.target == "enemy")
            {
                targetProfile.enemy = true;
            }

            if (effect.target == "player")
            {
                targetProfile.player = true;
            }
        }

        originalY = 0;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (potion != null)
        {
            GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(this);
        }
    }

    public string GetPotionTarget()
    {
        return potion.potion.effects[0].target;
    }

    public string GetPotionId()
    {
        return potion.id;
    }
}