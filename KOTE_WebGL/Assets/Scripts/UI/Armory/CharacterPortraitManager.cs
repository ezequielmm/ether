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
        {
            Debug.Log($"Parsing base64 string from {metadata.TokenId} {metadata.adaptedImageURI}");
            var base64Sprite = FromBase64ToSprite(metadata.adaptedImageURI);
            if (base64Sprite != null)
            {
                portraitImage.sprite = base64Sprite;
                return;
            }
            
            PortraitSpriteManager.Instance.GetKnightPortrait(metadata, sprite => {
                portraitImage.sprite = sprite;
            });
            
        }
        else
            portraitImage.sprite = villagerDefault;
    }

    private Sprite FromBase64ToSprite(string base64String)
    {

        if (string.IsNullOrEmpty(base64String)) {
            Debug.LogWarning("base64Image is empty. Please set a valid base64 image string in the Inspector.");
            return null;
        }

        // Decode the base64 string into a byte array
        var convert = base64String;
        convert = convert.Replace("data:image/png;base64,", "");
        convert = convert.Replace('-', '+');
        convert = convert.Replace('_', '/');
        byte[] imageBytes = null;
        
        try
        {
            imageBytes = Convert.FromBase64String(convert);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to parse base64 string: {e.Message}");
            return null;
        }

        // Create a Texture2D from the byte array
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(imageBytes)) {
            Debug.LogError("Failed to load the image from base64 string.");
            return null;
        }

        Debug.Log($"Parse success!");
        return texture.ToSprite();
    }
}