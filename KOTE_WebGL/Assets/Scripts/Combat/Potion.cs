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

    private void Start()
    {
        potionImage = GetComponent<Image>();
        potionButton = GetComponent<Button>();

        potionImage.sprite = unusedPotionSprite;
    }

    public void OnPotionUsed()
    {
        potionImage.sprite = usedPotionSprite;
        potionButton.interactable = false;

        GameManager.Instance.EVENT_POTION_USED.Invoke(this);
    }
}