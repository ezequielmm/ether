using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpriteManager : MonoBehaviour
{
   public Sprite HelmetSprite;
   private List<NftMetaData.Trait> currentMetadata = new List<NftMetaData.Trait>();

   private void Start()
   {
      GameManager.Instance.EVENT_NFT_METADATA_RECEIVED.AddListener(OnNftMetadataReceived);
      GameManager.Instance.EVENT_NFT_SKIN_SPRITE_RECEIVED.AddListener(OnSkinSpriteReceived);
      GameManager.Instance.EVENT_REQUEST_NFT_METADATA.Invoke(2702);
   }

   private void OnNftMetadataReceived(NftMetaData nftMetaData)
   {
      currentMetadata.Clear();
      currentMetadata.AddRange(nftMetaData.traits);
      NftMetaData.Trait curTrait = currentMetadata.Find(x => x.trait_type == "Helmet");
      string[] valueSplit = curTrait.value.Split();
      string imageName = curTrait.trait_type+ "s_" + string.Join('_', valueSplit) + ".png";
      Debug.Log($"imageName: {imageName}");
      GameManager.Instance.EVENT_REQUEST_NFT_SKIN_SPRITE.Invoke(imageName);
   }

   private void OnSkinSpriteReceived(Sprite skinSprite)
   {
      HelmetSprite = skinSprite;
   }
}
