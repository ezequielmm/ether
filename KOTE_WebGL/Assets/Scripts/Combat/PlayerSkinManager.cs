using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSkinManager : MonoBehaviour, IHasSkeletonDataAsset
{
    public SkeletonAnimation skeletonAnimation;
    public Material skeletonMaterial;
    public SkeletonDataAsset skeletonDataAsset;
    
    public UnityEvent skinLoaded = new();
    SkeletonDataAsset IHasSkeletonDataAsset.SkeletonDataAsset => skeletonDataAsset;

    private Skin equipsSkin;
    private SkeletonData skeletonData;
    private List<(Skin.SkinEntry, Attachment)> generatedAttachments;
    private Renderer renderer;

    public void SkinReset()
    {
        equipsSkin = new Skin("Equips");
        if (skeletonAnimation.skeleton == null)
        {
            Debug.LogWarning($"[PlayerSkinManager] Skeleton is null, can't apply skin");
            return;
        }
        skeletonAnimation.Skeleton.SetSkin(equipsSkin);

        RefreshSkeletonAttachments();
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        StartCoroutine(UpdateSkin());
    }

    private IEnumerator UpdateSkin()
    {
        //equip
        skeletonData = skeletonDataAsset.GetSkeletonData(true);
        
        List<TraitSprite> skinSprites = PlayerSpriteManager.Instance.GetAllTraitSprites();

        // some dark magic due to the way Flails are set up in spine
        //bool flailFound = TryStartWithFlailSkin(skinSprites, out equipsSkin);

        renderer ??= GetComponent<Renderer>();
        foreach (var mat in renderer.materials)
        {
            if (mat == null)
                continue;
            Destroy(mat);
        }

        foreach (var traitType in Enum.GetNames(typeof(Trait)))
        {
            // some dark magic due to the way Flails are set up in spine
            //if (traitType == "Weapon" && flailFound) continue;

            TraitSprite traitSprite = skinSprites.Find(x => x.TraitType.ToString() == traitType);
            if (string.IsNullOrEmpty(traitSprite?.SkinName))
            {
                // if this is called, nothing's going wrong, this nft doesn't have a skin for this trait, leaving for debugging
                Debug.LogWarning($"[PlayerSkinManager] Can't apply Sprite of type [{traitType}]: {traitSprite}");
                yield return null;
                continue;
            }
            
            var skin = skeletonData.FindSkin(traitSprite.SkinName);
            if (skin == null)
                Debug.Log("[UpdateSkin] skin" + traitSprite.SkinName + "NOT FOUND");
            else
                equipsSkin.AddSkin(skin);

            yield return null;
        }

        generatedAttachments = new List<(Skin.SkinEntry, Attachment)>();

        foreach (Skin.SkinEntry skinAttachment in equipsSkin.Attachments)
        {
            TraitSprite traitSprite = skinSprites.Find(x => x.AttachmentIndex == skinAttachment.SlotIndex);
            if (traitSprite == null) continue;
            // Debug.Log("[UpdateSkin] Attachment name is : " + skinAttachment.Attachment.Name);
            Sprite attachmentSprite = traitSprite.Sprite;
            string templateSkinName = traitSprite.SkinName;

            attachmentSprite.name = traitSprite.ImageName;

            if (templateSkinName != null)
            {
                Attachment attachment = GenerateAttachmentFromEquipAsset(attachmentSprite, skinAttachment.SlotIndex,
                    templateSkinName, skinAttachment.Name);

                generatedAttachments.Add((skinAttachment, attachment));
            }

            yield return null;
        }

        foreach ((Skin.SkinEntry, Attachment) attachmentData in generatedAttachments)
        {
            equipsSkin.SetAttachment(attachmentData.Item1.SlotIndex, attachmentData.Item1.Name, attachmentData.Item2);
            yield return null;
        }

        skeletonAnimation.Skeleton.SetSkin(equipsSkin);
        RefreshSkeletonAttachments();
        skinLoaded.Invoke();
        
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    /*
     * this is an extremely annoying case, but if the character is using a flail, the base skin
     * HAS to be the flail, or the bones won't exist, and the flail won't animate.
     * In all other cases it's perfectly fine to start from an empty skin
    */
    private bool TryStartWithFlailSkin(List<TraitSprite> skinSprites, out Skin outSkin)
    {
        TraitSprite traitSprite = skinSprites
            .Find(x => x.TraitType == Trait.Weapon && x.SkinName.Contains("Flail"));
        if (traitSprite != null)
        {
            Skin flailSkin = skeletonData.FindSkin(traitSprite?.SkinName);
            if (flailSkin != null)
            {
                outSkin = flailSkin;
                return true;
            }
        }

        outSkin = equipsSkin;
        return false;
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
        skeletonAnimation.Update(0);
    }
}