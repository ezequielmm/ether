using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitManager : MonoBehaviour
{
    public GameObject inactiveOverlay;
    [Header("Knight")] public GameObject knight;
    public Image knightImage;
    public Sprite defaultKnight;
    [Header("Villager")] public GameObject villager;
    // so we can deactivate the villager while it's loading
    public GameObject villagerComposite;
    public GameObject loadingText;
    public Image[] portraitLayers;

    public void SetDefault()
    {
        villager.SetActive(false);
        knight.SetActive(true);
        knightImage.sprite = defaultKnight;
    }

    public void SetPortrait(Nft metadata)
    {
        SetPlayableStatus(metadata.CanPlay);
        if (metadata.isKnight)
        {
            ShowKnightPortrait(metadata);
            return;
        }

        ShowVillagerPortrait(metadata);
    }

    private void SetPlayableStatus(bool canPlay)
    {
        if (canPlay)
        {
            inactiveOverlay.SetActive(false);
            return;
        }

        inactiveOverlay.SetActive(true);
    }

    private void ShowKnightPortrait(Nft metadata)
    {
        villager.SetActive(false);
        knight.SetActive(true);

        //TODO get correct knight image
        SetDefault();
    }

    private async void ShowVillagerPortrait(Nft metadata)
    {
        knight.SetActive(false);
        villager.SetActive(true);
        loadingText.SetActive(true);
        villagerComposite.SetActive(false);
        foreach (ImageSlot slot in Enum.GetValues(typeof(ImageSlot)))
        {
            Trait trait = TraitFromSlot(slot);
            if (!metadata.Traits.ContainsKey(trait)) continue;
            string traitName = metadata.Traits[trait];
            
            if (string.IsNullOrEmpty(traitName) || traitName == "None")
            {
                DeactivateLayer(slot);
                continue;
            }
            
            Sprite portraitSprite = await PortraitSpriteManager.Instance.GetPortraitSprite(trait, traitName);
            if (portraitSprite == null)
            {
                DeactivateLayer(slot);
                continue;
            }

            portraitLayers[(int)slot].gameObject.SetActive(true);
            portraitLayers[(int)slot].sprite = portraitSprite;
        }
        
        loadingText.SetActive(false);
        villagerComposite.SetActive(true);
    }

    private void DeactivateLayer(ImageSlot slot)
    {
        portraitLayers[(int)slot].gameObject.SetActive(false);
    }

    private Trait TraitFromSlot(ImageSlot slot)
    {
        return slot.ToString().ParseToEnum<Trait>();
    }

    // easy reference for knowing what image to update
    private enum ImageSlot
    {
        Padding = 0,
        Weapon = 1,
        Shield = 2,
        Helmet = 3
    }
}