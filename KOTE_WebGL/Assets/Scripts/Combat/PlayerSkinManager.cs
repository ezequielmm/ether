using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using UnityEngine;

public class PlayerSkinManager : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;
    public Material skeletonMaterial;
    public MeshRenderer meshRenderer;
    
    public AtlasAssetBase atlasBase;

    private Skeleton _skeleton;

    private SkeletonData _skeletonData;

    void Awake()
    {
        //GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.AddListener(UpdateSkin);
    }

    // Start is called before the first frame update
    void Start()
    {
        _skeleton = skeletonAnimation.skeleton;
        // SkeletonData is the data from the json, so we don't need to pull or parse it ourselves
        _skeletonData = _skeleton.Data;
        UpdateSkin();
    }

   private void UpdateSkin()
    {
        Skin playerSkin = new Skin("player_skin");
        /*
        playerSkin.AddSkin(_skeletonData.FindSkin("Boots/Boots_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Gauntlet/Gauntlet_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Weapon/Weapon_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Vambrace/Vambrace_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Padding/Padding_Brown"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Helmet/Helmet_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Shield/Shield_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Breastplate/Breastplate_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Crest/Crest_Red_Halo"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Pauldrons/Pauldrons_Medici"));
        playerSkin.AddSkin(_skeletonData.FindSkin("Legguard/Legguard_Medici"));
        // playerSkin.AddSkin(_skeletonData.FindSkin("Sigil/Sigil_Medici"));*/
        List<(Attachment, int)> UpdatedAttachmentList = new List<(Attachment, int)>();
        
        List<TraitSprite> skinSprites = PlayerSpriteManager.Instance.GetAllTraitSprites();
        foreach (TraitSprite traitSprite in skinSprites)
        {
            playerSkin.AddSkin(_skeletonData.FindSkin(traitSprite.skinName));
        }
        
        foreach (Skin.SkinEntry skinAttachment in playerSkin.Attachments)
        {
            Debug.Log(skinAttachment.Attachment.Name);
            Sprite attachmentSprite = PlayerSpriteManager.Instance.GetSpriteForAttachment(skinAttachment.SlotIndex);
            // if there's no sprite for that slot, ignore it
            if (attachmentSprite == null) continue;
            Attachment updatedAttachment =
                skinAttachment.Attachment.GetRemappedClone(attachmentSprite, skeletonMaterial,
                    useOriginalRegionSize: true);
            updatedAttachment.SetRegion(attachmentSprite.ToAtlasRegion(skeletonMaterial));

            UpdatedAttachmentList.Add((updatedAttachment, skinAttachment.SlotIndex));
        }

        // we have to do this separately as doing it in the above loop throws an error due to changing the data
        foreach ((Attachment, int) attachmentData in UpdatedAttachmentList)
        {
            playerSkin.SetAttachment(attachmentData.Item2, attachmentData.Item1.Name, attachmentData.Item1);
        }

        Material newMaterial;
        Texture2D newTexture;

        Skin repackedSkin =
            playerSkin.GetRepackedSkin("true player skin", skeletonMaterial, out newMaterial, out newTexture);
        _skeleton.SetSkin(playerSkin);
        _skeleton.SetSlotsToSetupPose();

       // meshRenderer.material.mainTexture = newTexture;
        skeletonAnimation.Update(0);
        Debug.Log("[PlayerSkinManager] Updating Player Skin");
    }
}