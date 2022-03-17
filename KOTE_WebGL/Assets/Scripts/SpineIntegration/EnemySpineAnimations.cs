using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;

public class EnemySpineAnimations : MonoBehaviour
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

    public void SetSkeletonDataAsset()
    {
        heroSkeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(animationJson, atlasAssetBase, true, 0.01f);
        heroSkeletonData = heroSkeletonDataAsset.GetSkeletonData(false);

        skeletonAnimationScript = GetComponent<SkeletonAnimation>() == null ? gameObject.AddComponent<SkeletonAnimation>() : GetComponent<SkeletonAnimation>();
        skeletonAnimationScript.skeletonDataAsset = heroSkeletonDataAsset;
        skeletonAnimationScript.Initialize(true);

        skeletonAnimationScript.AnimationState.AddAnimation(1, "flying", true, 0);

        animations = new List<string>();

        foreach (Animation animation in heroSkeletonData.Animations)
        {
            animations.Add(animation.Name);
        }
    }
}