using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.VFX;
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

    [SerializeField] private SkinVFXList skinsVFX;
    private SpineAnimationsManagement spine;

    public void SkinReset()
    {
        equipsSkin = new Skin("Equips");
        
        spine ??= GetComponentInChildren<SpineAnimationsManagement>();
        if (skeletonAnimation.skeleton == null)
        {
            if (spine != null){
                spine.SetSkeletonDataAsset();
            }
            else
            {
                Debug.LogWarning($"[PlayerSkinManager] Skeleton is null, can't apply skin");
                return;
            }
        }
        skeletonAnimation.Skeleton.SetSkin(equipsSkin);

        RefreshSkeletonAttachments();
        UpdateSkin();
    }

    private void UpdateSkin()
    {
        //equip
        skeletonDataAsset.Clear();
        skeletonData = skeletonDataAsset.GetSkeletonData(false);
        
        List<TraitSprite> skinSprites = PlayerSpriteManager.Instance.GetAllTraitSprites();
        
        var vfxInfo = new Dictionary<string, List<(GameObject, Transform)>>();
        foreach (var skin in PlayerSpriteManager.Instance.GetActiveSkinNames)
        {
            var vfxPair = skinsVFX.GetVFX(skin);
            if (vfxPair == null) continue;
            
            foreach (var vfxData in vfxPair)
            {
                var bone = spine.GetBone(vfxData.boneName);
                if (bone == null)
                    continue;
                
                if (!vfxInfo.ContainsKey(skin))
                    vfxInfo.Add(skin, new List<(GameObject, Transform)>());
                vfxInfo[skin].Add((vfxData.vfx, bone));
            }
        }
        
        // some dark magic due to the way Flails are set up in spine
        var flailFound = TryStartWithFlailSkin(skinSprites);
        if (flailFound != null)
        {
            equipsSkin = flailFound;
        }

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
            if (traitType == "Weapon" && flailFound != null)
            {
                continue;
            }

            
            TraitSprite traitSprite = skinSprites.Find(x => x.TraitType.ToString() == traitType);
            if (string.IsNullOrEmpty(traitSprite?.SkinName))
            {
                // if this is called, nothing's going wrong, this nft doesn't have a skin for this trait, leaving for debugging
                continue;
            }
            
            var skin = skeletonData.FindSkin(traitSprite.SkinName);
            if (skin == null)
                Debug.Log("[UpdateSkin] skin" + traitSprite.SkinName + "NOT FOUND");
            else
            {
                equipsSkin.AddSkin(skin);
            }
        }

        generatedAttachments = new List<(Skin.SkinEntry, Attachment)>();
        foreach (Skin.SkinEntry skinAttachment in equipsSkin.Attachments)
        {
            
            TraitSprite traitSprite = skinSprites.Find(x => x.AttachmentIndex == skinAttachment.SlotIndex);
            if (traitSprite == null)
            {
                continue;
            }
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
        }

        foreach ((Skin.SkinEntry, Attachment) attachmentData in generatedAttachments)
        {
            equipsSkin.SetAttachment(attachmentData.Item1.SlotIndex, attachmentData.Item1.Name, attachmentData.Item2);
        }
        
        skeletonAnimation.Skeleton.SetSkin(equipsSkin);
        RefreshSkeletonAttachments();
        
        ApplySkinVFX(vfxInfo);
        
        skinLoaded.Invoke();
    }

    private void ApplySkinVFX(Dictionary<string, List<(GameObject, Transform)>> dictionary)
    {
        StartCoroutine(ApplyVFXRoutine());
        IEnumerator ApplyVFXRoutine()
        {
            yield return new WaitForEndOfFrame();

            foreach (var kv in dictionary)
            {
                var visualEffect = skinsVFX.GetVFX(kv.Key);
                if (visualEffect != null)
                {
                    foreach (var tuple in kv.Value)
                    {
                        var vfx = Instantiate(tuple.Item1, tuple.Item2);
                        vfx.transform.localPosition = Vector3.zero;
                    }
                }
            }
        }
        
    }

    /*
     * this is an extremely annoying case, but if the character is using a flail, the base skin
     * HAS to be the flail, or the bones won't exist, and the flail won't animate.
     * In all other cases it's perfectly fine to start from an empty skin
     */
    private Skin TryStartWithFlailSkin(List<TraitSprite> skinSprites)
    {
        TraitSprite traitSprite = skinSprites
            .Find(x => x.TraitType == Trait.Weapon && x.SkinName.Contains("Flail"));
        if (traitSprite != null)
        {
            Skin flailSkin = skeletonData.FindSkin(traitSprite?.SkinName);
            if (flailSkin != null)
            {
                return flailSkin;
            }
        }

        return null;
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
            attachment.GetMaterial().mainTexture.name = sprite.texture.name;
            return attachment;
        }
        else if (templateAttachment.GetType() == typeof(MeshAttachment))
        {
            attachment = CreateMeshAttachment(templateAttachment, sprite);
            attachment.GetMaterial().mainTexture.name = sprite.texture.name;
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