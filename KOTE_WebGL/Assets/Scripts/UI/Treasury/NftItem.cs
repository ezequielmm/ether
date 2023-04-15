using UnityEngine;
using UnityEngine.UI;

public class NftItem : MonoBehaviour
{
    public Image nftImage;
    [HideInInspector]public Nft Nft;

    public async void Populate(Nft _nft)
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