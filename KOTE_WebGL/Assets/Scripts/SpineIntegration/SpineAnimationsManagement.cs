using System;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;
using UnityEngine.Events;

[Serializable, RequireComponent(typeof(SkeletonAnimation))]
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

    public UnityEvent<string> ANIMATION_EVENT = new UnityEvent<string>();

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
        if (skeletonAnimationScript?.skeletonDataAsset != null)
            return;
        SetProperties();
    }

    public void SetProperties()
    {
        skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(animationJson, atlasAssetBase, true);
        skeletonData = skeletonDataAsset.GetSkeletonData(false);

        skeletonAnimationScript = GetComponent<SkeletonAnimation>();
        skeletonAnimationScript.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimationScript.Initialize(true);

        availableAnimations.Clear();
        foreach (Animation animation in skeletonData.Animations)
        {
            availableAnimations.Add(animation.Name);
        }
    }

    /// <summary>
    /// Runs an animation by name
    /// </summary>
    /// <param name="animationSequenceName"></param>
    /// <returns>Duration of the animation</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public float PlayAnimationSequence(string animationSequenceName)
    {
        animationSequenceName = animationSequenceName.ToLower();
        foreach (AnimationSequence animationSequence in animations)
        {
            if (animationSequence.sequenceName.ToLower() == animationSequenceName)
            {
                float duration = 0;
                foreach (AnimationSequence.Animation animation in animationSequence.sequence)
                {
                    TrackEntry te = null;
                    switch (animation.animationEvent)
                    {
                        case AnimationEvent.Add:
                            te = skeletonAnimationScript.AnimationState.AddAnimation(animation.track, animation.name, animation.loop, animation.delay);
                            duration += te.Animation.Duration + animation.delay;
                            break;
                        case AnimationEvent.Set:
                            te = skeletonAnimationScript.AnimationState.SetAnimation(animation.track, animation.name, animation.loop);
                            duration = te.Animation.Duration;
                            break;
                        case AnimationEvent.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    if (te != null) 
                    {
                        te.Event += HandleEvent;
                    }
                }
                return duration;
            }
        }
        return 0;
    }

    private void HandleEvent(TrackEntry trackEntry, Spine.Event e) 
    {
        Debug.Log($"[Spine] Event Triggered: {e.Data.Name}");
        ANIMATION_EVENT.Invoke(e.Data.Name);
    }

    public float PlayAnimationSequence(AnimationSequence animationSequence)
    {
        return PlayAnimationSequence(animationSequence.sequenceName);
    }

    public void SetSkin(string skin)
    {
        skeletonAnimationScript.skeleton.SetSkin(skin);
    }
}