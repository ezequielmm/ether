using UnityEngine;
using UnityEngine.UI;

public class TreasuryNftItem : MonoBehaviour
{
    public Image nftImage;
    private NftMetaData _metaData;

    public void Populate(NftMetaData metaData)
    {
        _metaData = metaData;
        GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnImageReceived);
        GameManager.Instance.EVENT_REQUEST_NFT_IMAGE.Invoke(_metaData.token_id, _metaData.image_original_url);
    }

    private void OnImageReceived(string tokenId, Sprite image)
    {
        if (tokenId.Equals(_metaData.token_id))
        {
            nftImage.sprite = image;
        }
    }
}