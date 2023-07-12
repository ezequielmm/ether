using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitManager : MonoBehaviour
{
    public GameObject inactiveOverlay;

    public Image portraitImage;
    
    public Sprite defaultKnight;
    
    [SerializeField] private Sprite villagerDefault;

    public void SetDefault()
    {
        portraitImage.sprite = defaultKnight;
    }

    public void SetPortrait(Nft metadata)
    {
        SetPlayableStatus(metadata.CanPlay);
        ShowPortrait(metadata);
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
        if (!string.IsNullOrEmpty(metadata.adaptedImageURI))
            PortraitSpriteManager.Instance.GetKnightPortrait(metadata, sprite => {
                portraitImage.sprite = sprite;
            });
        else
            portraitImage.sprite = villagerDefault;
    }

}