using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.VFX;
using UnityEngine;
using Spine;
using Spine.Unity;
using TMPro;
using Animation = Spine.Animation;
using UnityEngine.Events;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("specialAnimation")] public AnimationType animationType = AnimationType.Other;

        [Serializable]
        public class Animation
        {
            public int track;
            public string name;
            public bool loop;
            public float delay;
            public AnimationEvent animationEvent;
        }

        public VFX[] vfxs;
    }
    public enum AnimationType
    {
        Idle,
        Death,
        Other
    }
    
    public UnityEvent<string> ANIMATION_EVENT = new UnityEvent<string>();

    private SkeletonData skeletonData;

    private SkeletonDataAsset skeletonDataAsset;
    private SkeletonAnimation skeletonAnimationScript;

    public TextAsset animationJson;
    public AtlasAssetBase atlasAssetBase;

    public SkeletonUtility skeletonUtility { get; private set; }
    

    [SerializeField]
    public List<AnimationSequence> animations = new List<AnimationSequence>();
    private Dictionary<string, AnimationSequence> animationsDictionary;
    
    private VFXList vfxList;
    
    
    [Unity.Collections.ReadOnly]
    public List<string> availableAnimations = new List<string>();
    
#if UNITY_EDITOR
    [ContextMenu("Reset Animations List")]
    private void ResetAnimationsList()
    {
        animations = new List<AnimationSequence>()
        {
            new AnimationSequence()
            {
                sequenceName = "idle",
                sequence = new List<AnimationSequence.Animation>()
                {
                    new AnimationSequence.Animation()
                    {
                        track = 0,
                        name = "idle",
                        loop = true,
                        delay = 0,
                        animationEvent = AnimationEvent.Set
                    }
                },
                animationType = AnimationType.Idle
            },
            new AnimationSequence()
            {
                sequenceName = "hit",
                sequence = new List<AnimationSequence.Animation>()
                {
                    new AnimationSequence.Animation()
                    {
                        track = 0,
                        name = "hit",
                        loop = false,
                        delay = 0,
                        animationEvent = AnimationEvent.Set
                    }
                }
            },
            new AnimationSequence()
            {
                sequenceName = "death",
                sequence = new List<AnimationSequence.Animation>()
                {
                    new AnimationSequence.Animation()
                    {
                        track = 0,
                        name = "death",
                        loop = false,
                        delay = 0,
                        animationEvent = AnimationEvent.Set
                    }
                },
                animationType = AnimationType.Death
            },
            new AnimationSequence()
            {
                sequenceName = "attack",
                sequence = new List<AnimationSequence.Animation>()
                {
                    new AnimationSequence.Animation()
                    {
                        track = 0,
                        name = "attack",
                        loop = false,
                        delay = 0,
                        animationEvent = AnimationEvent.Set
                    }
                }
            },
            new AnimationSequence()
            {
                sequenceName = "buff"
            },
            new AnimationSequence()
            {
                sequenceName = "debuff"
            },
            new AnimationSequence()
            {
                sequenceName = "defend"
            }
        };
    }
#endif
    
    public void Init(IIdleSolver idleSolver, VFXList vfxList = null)
    {
        this.vfxList = vfxList;
        this.idleSolver = idleSolver;
        animator = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
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

        skeletonAnimationScript = GetComponentInChildren<SkeletonAnimation>();
        skeletonAnimationScript.skeletonDataAsset = skeletonDataAsset;
        skeletonAnimationScript.Initialize(true);

        skeletonUtility = GetComponentInChildren<SkeletonUtility>();
        if (skeletonUtility)
            Debug.Log($"This getter call creates the skeleton {skeletonUtility.Skeleton}");
        skeletonUtility?.SpawnHierarchy(SkeletonUtilityBone.Mode.Follow, true, true, true);

        availableAnimations.Clear();
        foreach (Animation animation in skeletonData.Animations)
        {
            availableAnimations.Add(animation.Name);
        }
    }
    
    // This overload tries to keep a queue of animations, if queue isn't cleared the next incoming animation will be added to the queue
    private Queue<AnimationSequence> animationQueue = new();
    private Queue<string> vfxQueue = new();
    private IIdleSolver idleSolver;
    private Animator animator;
    private MeshRenderer meshRenderer;

    public float PlayAnimationSequence(string animationSequenceName, bool forceSet = false)
    {
        SetSkeletonDataAsset();
        
        animationSequenceName = animationSequenceName.ToLower();
        animationsDictionary ??= animations.ToDictionary(k => k.sequenceName.ToLower(), v => v);
        var foundKey = animationsDictionary.TryGetValue(animationSequenceName, out var animationSequence);
        
        float duration = 0;
        
        if (!foundKey){
            // HandleEvent(animationSequenceName, null, "attack");
            // return 0;
            animationSequence = new AnimationSequence()
            {
                sequenceName = "undefined",
                animationType = AnimationType.Other,
                sequence = new(),
                vfxs = new VFX[]{},
            };
            duration = .5f;
        }
        
        var sequenceRemovedSuccessfully = false;
        
        for (var i = 0; i < animationSequence.sequence.Count; i++)
        {
            var animation = animationSequence.sequence[i];
            TrackEntry te = null;
            if ((animation.animationEvent == AnimationEvent.Add || animationQueue.Count >= 1) && !forceSet)
            {
                if (!AnimationExists(animation.name))
                {
                    Debug.LogError($"Animation {animation.name} not found in {gameObject.name}");
                }
                else
                {
                    te = skeletonAnimationScript.AnimationState.AddAnimation(animation.track, animation.name,
                        animation.loop, animation.delay);
                    duration += te.Animation.Duration + animation.delay;
                }
            }
            else if (animation.animationEvent == AnimationEvent.Set)
            {
                if (!AnimationExists(animation.name))
                {
                    Debug.LogError($"Animation {animation.name} not found in {gameObject.name}");
                }
                else
                {
                    te = skeletonAnimationScript.AnimationState.SetAnimation(animation.track, animation.name,
                        animation.loop);
                    duration = te.Animation.Duration;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            if (te != null) {
                te.Event += (trackEntry, ev) => HandleEvent(animationSequenceName, trackEntry, ev.Data.Name);
                if (i == animationSequence.sequence.Count - 1)
                {
                    te.Event += (trackEntry, e) => {
                        Debug.Log($"[Spine] {gameObject.name} event fired {e.Data.Name}, removing from queue [duration: {duration}]");
                        sequenceRemovedSuccessfully = true;
                        RemoveFromQueue(animationSequenceName);
                    };
                }
            }
            
        }
        
        if (animationSequence.animationType != AnimationType.Idle)
            animationQueue.Enqueue(animationSequence);

        if (animationSequence.vfxs.Length > 0 && vfxList != null)
        {
            foreach (var vfx in animationSequence.vfxs)
            {
                var visualEffect = vfxList.GetVFX(vfx);
                if (!visualEffect) continue;
                
                vfxQueue.Enqueue(vfx.ToString());
                if (vfxQueue.Count <= 1)
                    PlayNextVfx();
            }
        }
        
        StartCoroutine(CheckRemovedFromQueue(duration));
        IEnumerator CheckRemovedFromQueue(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!sequenceRemovedSuccessfully)
                RemoveFromQueue(animationSequenceName);
        }
        
        return duration;
    }

    private void PlayNextVfx()
    {
        if (vfxQueue.Count <= 0) return;
        
        if (Enum.TryParse<VFX>(vfxQueue.Dequeue(), out var vfx))
            vfxList.GetVFX(vfx)?.Play(this, animator, meshRenderer, PlayNextVfx);
    }
    
    private void RemoveFromQueue(string sequenceName)
    {
        if (animationQueue.Count <= 0) return;
        
        var sequence = animationQueue.Dequeue();

        if (animationQueue.Count <= 0)
            // TODO: awful hack to prevent idle animation from playing when death animation is playing
        {
            PlayIdle(sequence.animationType == AnimationType.Death);
        }
    }

    public void PlayIdle(bool isDied = false)
    {
        if (isDied || animationQueue.Count > 0)
            return;

        var selectIdleSequence = idleSolver.DetermineIdleSequence();
        animationsDictionary ??= animations.ToDictionary(k => k.sequenceName.ToLower(), v => v);
        if (!animationsDictionary.TryGetValue(selectIdleSequence, out var animationSequence))
        {
            selectIdleSequence = selectIdleSequence.ToLower();
            if (!animationsDictionary.TryGetValue(selectIdleSequence, out animationSequence)) {
                Debug.LogError($"idle animation {selectIdleSequence} not found in {gameObject.name}");
                return;
            }
        }

        var idleVfx = idleSolver.DetermineIdleVFX();
        if (idleVfx != VFX.None) {
            var visualEffect = vfxList.GetVFX(idleVfx);
            if (visualEffect)
                visualEffect.Play(this, animator, meshRenderer);
        }
        
        PlayAnimationSequence(animationSequence.sequenceName);
    }

    private bool AnimationExists(string animationName) => 
        skeletonAnimationScript.skeleton.Data.FindAnimation(animationName) != null;

    private void HandleEvent(string sequenceName, TrackEntry trackEntry, string eventName) 
    {
        Debug.Log($"[Spine] Event Triggered: {eventName}");
        ANIMATION_EVENT.Invoke(eventName);
    }

    public float PlayAnimationSequence(AnimationSequence animationSequence)
    {
        return PlayAnimationSequence(animationSequence.sequenceName);
    }

    public void SetSkin(string skin)
    {
        skeletonAnimationScript.skeleton.SetSkin(skin);
    }

    public Transform GetBone(string boneName) => skeletonUtility != null ? skeletonUtility.boneComponents.FirstOrDefault(b => b.name == boneName)?.transform : null;
}