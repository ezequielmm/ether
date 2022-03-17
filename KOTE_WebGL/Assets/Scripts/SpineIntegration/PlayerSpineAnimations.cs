using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Spine;
using Spine.Unity;
using UnityEditor.VersionControl;
using UnityEngine;
using Animation = Spine.Animation;
using Vector2 = UnityEngine.Vector2;

public class PlayerSpineAnimations : MonoBehaviour
{
    private SkeletonData heroSkeletonData;

    private SkeletonDataAsset heroSkeletonDataAsset;
    private SkeletonAnimation skeletonAnimationScript;

    public TextAsset animationJson;
    public AtlasAssetBase atlasAssetBase;

    public List<string> animations;

    private bool attacking;

    private void Start()
    {
        SetSkeletonDataAsset();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectMouseClick();
        }
    }

    public void SetSkeletonDataAsset()
    {
        heroSkeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(animationJson, atlasAssetBase, true, 0.01f);
        heroSkeletonData = heroSkeletonDataAsset.GetSkeletonData(false);

        skeletonAnimationScript = GetComponent<SkeletonAnimation>() == null ? gameObject.AddComponent<SkeletonAnimation>() : GetComponent<SkeletonAnimation>();
        skeletonAnimationScript.skeletonDataAsset = heroSkeletonDataAsset;
        skeletonAnimationScript.Initialize(true);
        skeletonAnimationScript.skeleton.SetSkin("weapon/sword");

        skeletonAnimationScript.AnimationState.AddAnimation(1, "idle", true, 0);

        animations = new List<string>();

        foreach (Animation animation in heroSkeletonData.Animations)
        {
            animations.Add(animation.Name);
        }
    }

    public void DetectMouseClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Attack();
        }
    }

    public void Attack()
    {
        skeletonAnimationScript.AnimationState.SetAnimation(1, "attack", false);
        skeletonAnimationScript.AnimationState.AddAnimation(1, "idle", true, 0);
    }
}