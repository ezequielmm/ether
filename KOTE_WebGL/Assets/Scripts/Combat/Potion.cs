using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Potion : MonoBehaviour
{
    public Sprite unusedPotionSprite, usedPotionSprite;
    private Image potionImage;
    private Button potionButton;

    private TooltipAtCursor tooltipController;

    private Tooltip unknown;

    private TargetProfile targetProfile;

    private float SizeOnHover = 1.3f;
    private float originalY;


    private void Start()
    {
        tooltipController = GetComponent<TooltipAtCursor>();
        unknown = new Tooltip()
        {
            title = "Mysterious Elixir",
            description = "Cause an unknown effect to an enemy."
        };
        potionImage = GetComponent<Image>();
        potionButton = GetComponent<Button>();

        potionImage.sprite = unusedPotionSprite;
        tooltipController.SetTooltips(new List<Tooltip>() { unknown });
        targetProfile = new TargetProfile()
        {
            player = true,
            enemy = true
        };
        originalY = 0;
    }

    private void OnPotion()
    {
        potionImage.sprite = usedPotionSprite;
        tooltipController.SetTooltips(null);
    }

    private void OnPotionUsed(GameObject invoker)
    {
        if (invoker != gameObject) return;

        Debug.Log($"Potion used");

        potionImage.sprite = usedPotionSprite;
        potionButton.interactable = false;

        GameManager.Instance.EVENT_POTION_USED.Invoke(this);
    }


    // These are controlled via the event system
    public void DragStart() 
    {
        originalY = transform.position.y;
        transform.DOScale(SizeOnHover, 0.2f);
        transform.DOMoveY(originalY + 5f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void OnDrag()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(transform.position);
        position.z = 0;

        PointerData pd = new PointerData(position, PointerOrigin.potion, targetProfile);

        GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(pd);
        GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(false);
    }

    public void DragEnd() 
    {
        Debug.LogWarning($"[Postion] Potion needs potion ID to use the potion.");
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.Invoke("PostionID");
        GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(true);
        DOTween.Kill(transform);
        transform.DOScale(1f, 0.2f);
        transform.DOMoveY(originalY, 0.2f).SetEase(Ease.InOutSine);
    }
}