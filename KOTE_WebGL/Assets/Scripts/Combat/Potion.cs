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
        GameManager.Instance.EVENT_TARGET_POINTER_SELECTED.AddListener(OnPotionUsed);

        potionImage = GetComponent<Image>();
        potionButton = GetComponent<Button>();

        potionImage.sprite = unusedPotionSprite;
    }

    public void OnPotion()
    {
        GameManager.Instance.EVENT_START_POINTER.Invoke(gameObject);
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