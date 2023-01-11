using UnityEngine;
using UnityEngine.UI;

public class TreasuryNftItem : MonoBehaviour
{
    public Image nftImage;
    [HideInInspector]public NftMetaData metaData;

    public void Populate(NftMetaData metaData)
    {
        this.metaData = metaData;
        GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnImageReceived);
        GameManager.Instance.EVENT_REQUEST_NFT_IMAGE.Invoke(this.metaData.token_id, this.metaData.image_original_url);
    }

    private void OnImageReceived(string tokenId, Sprite image)
    {
        if (tokenId.Equals(metaData.token_id))
        {
            nftImage.sprite = image;
        }
    }
}