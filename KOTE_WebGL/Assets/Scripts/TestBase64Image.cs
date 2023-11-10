using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class TestBase64Image : MonoBehaviour
    {
        [SerializeField] private string imageStr;
        [SerializeField] private Image targetImage;

        private void Awake()
        {
            // byte[]  imageBytes = Convert.FromBase64String(imageStr);
            // Texture2D tex = new Texture2D(2, 2);
            // tex.LoadImage( imageBytes );
            // Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            //
            // targetImage.sprite = sprite;
            Try();
        }
        private void Try()
        {
            if (!string.IsNullOrEmpty(imageStr))
            {
                // Decode the base64 string into a byte array

                var convert = imageStr;
                convert = convert.Replace("data:image/png;base64,", "");
                convert = convert.Replace('-', '+');
                convert = convert.Replace('_', '/');
                byte[] imageBytes = Convert.FromBase64String(convert);

                // Create a Texture2D from the byte array
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    // Apply the texture to a material
                    targetImage.sprite = texture.ToSprite();
                }
                else
                {
                    Debug.LogError("Failed to load the image from base64 string.");
                }
            }
            else
            {
                Debug.LogWarning("base64Image is empty. Please set a valid base64 image string in the Inspector.");
            }
        }
    }
}