using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

[assembly: InternalsVisibleTo("PlayModeTests")]

namespace KOTE.UI.Armory
{
    internal class ArmoryTokenData
    {
        // this is so the Armory panel knows if image is updated after showing the character
        public UnityEvent tokenImageReceived = new();

        public string Id => MetaData.token_id;
        public NftMetaData MetaData { get; }
        public Sprite NftImage { get; private set; }

        public ArmoryTokenData(NftMetaData tokenMetadata)
        {
            MetaData = tokenMetadata;
            if (NftImageManager.Instance.TryGetNftImage(MetaData, out Sprite image))
            {
                NftImage = image;
                return;
            }

            NftImage = image;
            // if the image isn't already downloaded, wait for it to be
            GameManager.Instance.EVENT_NFT_IMAGE_RECEIVED.AddListener(OnImageReceived);
        }

        private void OnImageReceived(string tokenId, Sprite image)
        {
            if (tokenId.Equals(MetaData.token_id))
            {
                NftImage = image;
            }

            tokenImageReceived.Invoke();
        }
    }
}