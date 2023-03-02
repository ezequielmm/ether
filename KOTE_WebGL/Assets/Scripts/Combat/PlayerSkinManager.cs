using System;
using System.Collections.Generic;
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
        GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.AddListener(SkinReset);
        SkinReset();
    }

    private void SkinReset() 
    {
        equipsSkin = new Skin("Equips");
        skeletonAnimation.Skeleton.SetSkin(equipsSkin);

        RefreshSkeletonAttachments();
        UpdateSkin();
    }

    private void UpdateSkin()
    {
        //equip
        skeletonData = skeletonDataAsset.GetSkeletonData(true);
        Debug.Log("[UpdateSkin] skeletonData:" + skeletonData);

        List<TraitSprite> skinSprites = PlayerSpriteManager.Instance.GetAllTraitSprites();
        foreach (var traitType in Enum.GetNames(typeof(Trait)))
        {
            TraitSprite traitSprite = skinSprites.Find(x => x.traitType == traitType);
            if (string.IsNullOrEmpty(traitSprite.skinName)) continue;
            Debug.Log("[UpdateSkin] traitSprite.skinName:" + traitSprite.skinName);
            Skin skin = skeletonData.FindSkin(traitSprite.skinName);
            if (skin == null)
            {
                Debug.Log("[UpdateSkin] skin" + traitSprite.skinName + "NOT FOUND");
            }
            else
            {
                Debug.Log(
                    $"[UpdateSkin] ADDING SKIN : {traitSprite.skinName} WITH ATTACHMENT {traitSprite.attachmentIndex} AND IMAGE {traitSprite.imageName}");
                equipsSkin.AddSkin(skin);
            }
        }

        List<(Skin.SkinEntry, Attachment)> generatedAttachments = new List<(Skin.SkinEntry, Attachment)>();

        foreach (Skin.SkinEntry skinAttachment in equipsSkin.Attachments)
        {
            TraitSprite traitSprite = skinSprites.Find(x => x.attachmentIndex == skinAttachment.SlotIndex);
            // Debug.Log("[UpdateSkin] Attachment name is : " + skinAttachment.Attachment.Name);
            Sprite attachmentSprite = traitSprite.sprite;
            string templateSkinName = traitSprite.skinName;

            spritesArray.Add(attachmentSprite);

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

        var templateSkin = skeletonData.FindSkin(templateSkinName);
        Attachment templateAttachment = templateSkin.GetAttachment(slotIndex, templateAttachmentName);

        if (templateAttachment.GetType() == typeof(RegionAttachment))
        {
            attachment = CreateRegionAttachment(templateAttachment, sprite);
            return attachment;
        }
        else if (templateAttachment.GetType() == typeof(MeshAttachment))
        {
            attachment = CreateMeshAttachment(templateAttachment, sprite);
            return attachment;
        }

        // Note: Each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true` creates
        // a cached Texture copy which can be cleared by calling AtlasUtilities.ClearCache() as shown in the method below.
        Debug.LogWarning("[UpdateSkin] Warning! Skin attachment not a mesh or a region!");
        return null;
    }

    private Attachment CreateRegionAttachment(Attachment templateAttachment, Sprite sprite)
    {
        Attachment attachment;
        RegionAttachment baseAttachment = (RegionAttachment)templateAttachment;

        attachment = templateAttachment.GetRemappedClone(sprite, skeletonMaterial, premultiplyAlpha: true,
            useOriginalRegionSize: true, useOriginalRegionScale: true);
        RegionAttachment newAttachment = (RegionAttachment)attachment;
        newAttachment.RegionHeight = baseAttachment.RegionHeight;
        newAttachment.RegionWidth = baseAttachment.RegionWidth;
        newAttachment.RegionOffsetX = baseAttachment.RegionOffsetX;
        newAttachment.RegionOffsetY = baseAttachment.RegionOffsetY;
        for (int i = 0; i < baseAttachment.Offset.Length; i++)
        {
            newAttachment.Offset[i] = baseAttachment.Offset[i];
        }

        return newAttachment;
    }

    private Attachment CreateMeshAttachment(Attachment templateAttachment, Sprite sprite)
    {
        Attachment attachment;
        MeshAttachment baseAttachment = (MeshAttachment)templateAttachment;
        attachment = templateAttachment.GetRemappedClone(sprite, skeletonMaterial, premultiplyAlpha: true,
            useOriginalRegionSize: true, useOriginalRegionScale: true);
        MeshAttachment newAttachment = (MeshAttachment)attachment;

        newAttachment.RegionOffsetX = baseAttachment.RegionOffsetX;
        newAttachment.RegionOffsetY = baseAttachment.RegionOffsetY;
        newAttachment.UVs = baseAttachment.UVs;
        newAttachment.Triangles = baseAttachment.Triangles;
        newAttachment.Edges = baseAttachment.Edges;
        newAttachment.RegionUVs = baseAttachment.RegionUVs;


        newAttachment.UpdateUVs();
        return attachment;
    }

    void RefreshSkeletonAttachments()
    {
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        //skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.Update(0);
    }
}