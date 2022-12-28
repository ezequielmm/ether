using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PlayerSkinManager : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;

    public SpineAnimationsManagement spineManagement;

    [SpineSkin] public string skinName;
    [SpineSlot] public string slotName;

    [SpineAttachment(slotField: "slotName", skinField: "skinName")] public string AttachmentKey;
    private Skeleton _skeleton;

    private SkeletonData _skeletonData;
    // Start is called before the first frame update
    void Start()
    {
         _skeleton = skeletonAnimation.skeleton;
         _skeletonData = _skeleton.Data;
         foreach (SlotData slot in _skeletonData.Slots)
         {
             Debug.LogError($"Slot {slot.Name}");
         }

         foreach (Skin skin in _skeletonData.Skins)
         {
             if (skin.Name == "default")
             {
                 
                 foreach (Skin.SkinEntry attachment in skin.Attachments)
                 {
                     Debug.LogError($"Skin {attachment.Attachment.Name}");
                 }
             }
         }
    }
}
