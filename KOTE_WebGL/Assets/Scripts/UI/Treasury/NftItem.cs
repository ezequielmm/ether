using UnityEngine;
using UnityEngine.UI;

public class NftItem : MonoBehaviour
{
    public Image nftImage;
    [HideInInspector]public Nft Nft;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void Populate(Nft _nft)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        this.Nft = _nft;
        // TODO REENABLE ONCE THE TREASURY IS USED AGAIN
        /*
        Sprite image = await Nft.GetImage();
        if (image != null)
        {
            nftImage.sprite = image;
            return;
        }
        */
    }
}