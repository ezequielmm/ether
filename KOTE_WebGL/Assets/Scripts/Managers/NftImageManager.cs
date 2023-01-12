using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NftImageManager : SingleTon<NftImageManager>
{
    private List<string> requestedImages = new List<string>();
    private List<nftSprite> imageCache = new List<nftSprite>();

    private struct nftSprite
    {
        public string tokenId;
        public Sprite tokenImage;
    }

    void Start()
    {
        GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(OnMetadataReceived);
        GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnSpriteReceived);
    }

    private void OnMetadataReceived(NftData nftData)
    {
        List<NftMetaData> tokensToRequest = new List<NftMetaData>();
        foreach (NftMetaData metaData in nftData.assets)
        {
            if (!requestedImages.Contains(metaData.token_id))
            {
                requestedImages.Add(metaData.token_id);
                tokensToRequest.Add(metaData);
            }
        }

        GameManager.Instance.EVENT_REQUEST_NFT_IMAGE.Invoke(tokensToRequest.ToArray());
    }

    private void OnSpriteReceived(string tokenId, Sprite tokenImage)
    {
        imageCache.Add(new nftSprite { tokenId = tokenId, tokenImage = tokenImage });
    }

    // returns the image for the requested tokenId if it has been downloaded already
    public Sprite GetNftImage(NftMetaData metaData)
    {
        if (imageCache.Exists(x => x.tokenId == metaData.token_id))
        {
            return imageCache.Find(x => x.tokenId == metaData.token_id).tokenImage;
        }

        return null;
    }
}