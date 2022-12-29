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

    void Awake()
    {
        GameManager.Instance.EVENT_UPDATE_PLAYER_SKIN.AddListener(UpdateSkin);
    }
    
    // Start is called before the first frame update
    void Start()
    {
         _skeleton = skeletonAnimation.skeleton;
         _skeletonData = _skeleton.Data;
         UpdateSkin();
    }

    private void UpdateSkin()
    {
        Debug.Log("[PlayerSkinManager] Updating Player Skin");
        Sprite helmetSprite = PlayerSpriteManager.Instance.GetSpriteForTrait("Helmet");
        Debug.Log($"[PlayerSkinManager] Received Helmet sprite {helmetSprite.name}");
    }
}
