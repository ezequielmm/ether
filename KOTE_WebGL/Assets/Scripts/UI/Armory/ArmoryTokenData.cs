using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KOTE.UI.Armory
{
    internal class ArmoryTokenData
    {
        public string Id => MetaData.token_id;
        public NftMetaData MetaData { get; }
        public Sprite NftImage { get; private set; }

        public ArmoryTokenData(NftMetaData tokenMetadata)
        {
            MetaData = tokenMetadata;
            Sprite image = NftImageManager.Instance.GetNftImage(MetaData);
            if (image != null)
            {
                NftImage = image;
                return;
            }

            // if the image isn't already downloaded, wait for it to be
            GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnImageReceived);
        }

        private void OnImageReceived(string tokenId, Sprite image)
        {
            if (tokenId.Equals(MetaData.token_id))
            {
                NftImage = image;
            }
        }
    }
}
