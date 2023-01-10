using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using UnityEngine;

public class PlayerSkinManager : MonoBehaviour, IHasSkeletonDataAsset
{
    public SkeletonAnimation skeletonAnimation;
    public Material skeletonMaterial;

    private SkeletonData _skeletonData;

    public SkeletonDataAsset skeletonDataAsset;

    public List<Sprite> spritesArray = new List<Sprite>();
    SkeletonDataAsset IHasSkeletonDataAsset.SkeletonDataAsset => skeletonDataAsset;

    Skin equipsSkin;
    SkeletonData skeletonData;


    // Start is called before the first frame update
    void Start()
    {
        equipsSkin = new Skin("Equips");
        skeletonAnimation.Skeleton.Skin = equipsSkin;
        
        RefreshSkeletonAttachments();
        UpdateSkin();
    }

    private void UpdateSkin()
    {
        //equip
        skeletonData = skeletonDataAsset.GetSkeletonData(true);
        Debug.Log("[UpdateSkin] skeletonData:" + skeletonData);

        List<TraitSprite> skinSprites = PlayerSpriteManager.Instance.GetAllTraitSprites();
        foreach (TraitSprite traitSprite in skinSprites)
        {
            // Debug.Log("[UpdateSkin] traitSprite.skinName:" + traitSprite.skinName);
            Skin skin = skeletonData.FindSkin(traitSprite.skinName);
            if (skin == null)
            {
                Debug.Log("[UpdateSkin] skin" + traitSprite.skinName + "NOT FOUND");
            }
            else
            {
                Debug.Log($"[UpdateSkin] ADDING SKIN : {traitSprite.skinName} WITH ATTACHMENT {traitSprite.attachmentIndex} AND IMAGE {traitSprite.imageName}");
                equipsSkin.AddSkin(skin);
            }
        }

        List<(Skin.SkinEntry, Attachment)> generatedAttachments = new List<(Skin.SkinEntry, Attachment)>();
        
        foreach (Skin.SkinEntry skinAttachment in equipsSkin.Attachments)
        {
           // Debug.Log("[UpdateSkin] Attachment name is : " + skinAttachment.Attachment.Name);

            Sprite attachmentSprite = PlayerSpriteManager.Instance.GetSpriteForAttachment(skinAttachment.SlotIndex);
            spritesArray.Add(attachmentSprite);

            string templateSkinName = PlayerSpriteManager.Instance.GetSkinName(skinAttachment.SlotIndex);

            if (templateSkinName != null)
            {
                Attachment attachment = GenerateAttachmentFromEquipAsset(attachmentSprite, skinAttachment.SlotIndex,
                    templateSkinName, skinAttachment.Name);

                generatedAttachments.Add((skinAttachment, attachment));
            }
        }

        foreach ((Skin.SkinEntry, Attachment) attachmentData in generatedAttachments)
        {
            equipsSkin.SetAttachment(attachmentData.Item1.SlotIndex, attachmentData.Item1.Name, attachmentData.Item2);
            skeletonAnimation.Skeleton.SetSkin(equipsSkin);
        }
        
        RefreshSkeletonAttachments();
    }

    Attachment GenerateAttachmentFromEquipAsset(Sprite sprite, int slotIndex, string templateSkinName,
        string templateAttachmentName)
    {
        Attachment attachment;

        var skeletonData = skeletonDataAsset.GetSkeletonData(true);
        var templateSkin = skeletonData.FindSkin(templateSkinName);
        Attachment templateAttachment = templateSkin.GetAttachment(slotIndex, templateAttachmentName);
        attachment = templateAttachment.GetRemappedClone(sprite, skeletonMaterial, premultiplyAlpha: true);
        // Note: Each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true` creates
        // a cached Texture copy which can be cleared by calling AtlasUtilities.ClearCache() as shown in the method below.
        return attachment;
    }

    void RefreshSkeletonAttachments()
    {
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.Update(0);
    }
}