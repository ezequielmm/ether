using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Spine;
using Spine.Unity;
using Unity.Collections;
using Animation = Spine.Animation;

[Serializable]
public class SpineAnimationsManagement : MonoBehaviour
{
    public enum AnimationEvent
    {
        Add,
        Set,
        None
    }

    [Serializable]
    public class AnimationSequence
    {
        public string sequenceName;

        [SerializeField]
        public List<Animation> sequence;

        [Serializable]
        public class Animation
        {
            public int track;

            public string name;

            public bool loop;
            public float delay;
            public AnimationEvent animationEvent;
        }
    }

    private SkeletonData skeletonData;

    private SkeletonDataAsset skeletonDataAsset;
    private SkeletonAnimation skeletonAnimationScript;

    public TextAsset animationJson;
    public AtlasAssetBase atlasAssetBase;

    [SerializeField]
    public List<AnimationSequence> animations = new List<AnimationSequence>();

    [Unity.Collections.ReadOnly]
    public List<string> availableAnimations = new List<string>();

    private void Awake()
    {
        SetSkeletonDataAsset();
    }

    public void SetSkeletonDataAsset()
    {
        SetProperties();
    }

    public void SetProperties()
    {
        skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(animationJson, atlasAssetBase, true);
        skeletonData = skeletonDataAsset.GetSkeletonData(false);

        skeletonAnimationScript = gameObject.GetComponent<SkeletonAnimation>() == null ? gameObject.AddComponent<SkeletonAnimation>() : GetComponent<SkeletonAnimation>();
        skeletonAnimationScript.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimationScript.Initialize(true);

        availableAnimations.Clear();
        foreach (Animation animation in skeletonData.Animations)
        {
            availableAnimations.Add(animation.Name);
        }
    }

    public void PlayAnimationSequence(string animationSequenceName)
    {
        foreach (AnimationSequence animationSequence in animations)
        {
            if (animationSequence.sequenceName == animationSequenceName)
            {
                foreach (AnimationSequence.Animation animation in animationSequence.sequence)
                {
                    switch (animation.animationEvent)
                    {
                        case AnimationEvent.Add:
                            skeletonAnimationScript.AnimationState.AddAnimation(animation.track, animation.name, animation.loop, animation.delay);
                            break;
                        case AnimationEvent.Set:
                            skeletonAnimationScript.AnimationState.SetAnimation(animation.track, animation.name, animation.loop);
                            break;
                        case AnimationEvent.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return;
            }
        }
    }

    public void PlayAnimationSequence(AnimationSequence animationSequence)
    {
        foreach (AnimationSequence animationSequenceInList in animations)
        {
            if (animationSequenceInList.sequenceName == animationSequence.sequenceName)
            {
                foreach (AnimationSequence.Animation animation in animationSequenceInList.sequence)
                {
                    switch (animation.animationEvent)
                    {
                        case AnimationEvent.Add:
                            skeletonAnimationScript.AnimationState.AddAnimation(animation.track, animation.name, animation.loop, animation.delay);
                            break;
                        case AnimationEvent.Set:
                            skeletonAnimationScript.AnimationState.SetAnimation(animation.track, animation.name, animation.loop);
                            break;
                        case AnimationEvent.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return;
            }
        }
    }

    public void SetSkin(string skin)
    {
        skeletonAnimationScript.skeleton.SetSkin(skin);
    }
}