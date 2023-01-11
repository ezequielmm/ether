using UnityEngine;
using UnityEngine.UI;

public class TreasuryNftItem : MonoBehaviour
{
    public Image nftImage;
    [HideInInspector]public NftMetaData metaData;

    public void Populate(NftMetaData metaData)
    {
        this.metaData = metaData;
        Sprite image = NftImageManager.Instance.GetNftImage(metaData);
        if (image != null)
        {
            nftImage.sprite = image;
            return;
        }
        // if the image isn't already downloaded, wait for it to be
        GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnImageReceived);
    }

    private void OnImageReceived(string tokenId, Sprite image)
    {
        if (tokenId.Equals(metaData.token_id))
        {
            nftImage.sprite = image;
        }
    }
}