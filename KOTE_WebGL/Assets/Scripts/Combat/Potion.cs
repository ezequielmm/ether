using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Potion : MonoBehaviour
{
    public Sprite unusedPotionSprite, usedPotionSprite;
    private Image potionImage;
    private Button potionButton;

    private TooltipAtCursor tooltipController;

    private Tooltip unknown;

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
    }

    public void OnPotion()
    {
        potionImage.sprite = usedPotionSprite;
        tooltipController.SetTooltips(null);
    }

    public void OnPotionUsed(GameObject invoker)
    {
        if (invoker != gameObject) return;

        Debug.Log($"Potion used");

        potionImage.sprite = usedPotionSprite;
        potionButton.interactable = false;

        GameManager.Instance.EVENT_POTION_USED.Invoke(this);
    }
}