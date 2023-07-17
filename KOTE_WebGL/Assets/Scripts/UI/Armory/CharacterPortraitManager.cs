using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitManager : MonoBehaviour
{
    public GameObject inactiveOverlay;

    public Image portraitImage;
    
    public Sprite defaultKnight;
    public GameObject selectedFrame;
    
    [SerializeField] private Sprite villagerDefault;
    private Nft nft;

    public void SetDefault()
    {
        portraitImage.sprite = defaultKnight;
    }

    public void SetPortrait(Nft metadata)
    {
        SetPlayableStatus(metadata.CanPlay);
        ShowPortrait(metadata);
    }

    internal void SetSelected(bool selected)
    {
        selectedFrame.SetActive(selected);

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

    private void ShowPortrait(Nft metadata)
    {
        this.nft = metadata;
        if (!string.IsNullOrEmpty(metadata.adaptedImageURI))
            PortraitSpriteManager.Instance.GetKnightPortrait(metadata, sprite => {
                portraitImage.sprite = sprite;
            });
        else
            portraitImage.sprite = villagerDefault;
    }

}