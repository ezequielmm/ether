using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestroyOnGameStatus : MonoBehaviour
{
    public List<CauseOfDeath> causesOfDeath = new List<CauseOfDeath>();

    [System.Serializable]
    public class CauseOfDeath 
    {
        [Header("Status")]
        public GameStatuses StatusToListenTo = GameStatuses.GameOver;

        [Header("Death Length")]
        [Tooltip("The time it takes to run an animation before destroying itself.")]
        public float AnimationTime = 1f;

        [Header("Shrink On Death")]
        [Tooltip("Upon message, this object will shrink before destroying itself.")]
        public bool ShrinkToDie = true;

        [Header("Move On Death")]
        [Tooltip("Upon message, this object will move by the provided position.")]
        public bool MoveToDie = false;
        [Tooltip("The vector to move by")]
        public Vector3 MoveBy;
        [Tooltip("When true, MoveBy will be multiplied by the item's width")]
        public bool MoveMultipleOfSelf = false;

        [Header("Make Component Global")]
        [Tooltip("Unparents itself upon message.")]
        public bool UnParent = false;

        [Header("Don't Destroy")]
        [Tooltip("If true, the item will only be disabled and will not be destroyed.")]
        public bool DisableNotDestroy = false;
    }

    bool destroyCalled = false;
    private void Awake()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(onGameChange);
    }

    void onGameChange(GameStatuses newState) 
    {
        foreach (CauseOfDeath death in causesOfDeath) 
        {
            if (newState == death.StatusToListenTo)
            {
                if (death.UnParent)
                {
                    transform.parent = null;
                }
                StartCoroutine(DestroySelf(death));
            }
        }
    }

    IEnumerator DestroySelf(CauseOfDeath death) 
    {
        if (death.ShrinkToDie) 
        {
            transform.DOScale(Vector3.zero, death.AnimationTime).OnComplete(() => { Destroy(death.DisableNotDestroy); });
        }

        if (death.MoveToDie) 
        {
            Vector3 moveAmount = death.MoveBy;
            if (death.MoveMultipleOfSelf) 
            {
                var rectTrans = GetComponent<RectTransform>();
                if (rectTrans != null) 
                {
                    moveAmount = new Vector3(death.MoveBy.x * rectTrans.rect.width, death.MoveBy.y * rectTrans.rect.height, death.MoveBy.z * 0);
                }
            }
            transform.DOMove(transform.position + moveAmount, death.AnimationTime).OnComplete(() => { Destroy(death.DisableNotDestroy); });
        }

        yield return new WaitForSeconds(death.AnimationTime);
        Destroy(death.DisableNotDestroy);
    }

    void Destroy(bool DisableNotDestroy) 
    {
        if (!destroyCalled) 
        {
            destroyCalled = true;
            DOTween.Kill(gameObject);
            if (!DisableNotDestroy)
            {
                Destroy(gameObject);
            }
            else 
            {
                gameObject.SetActive(false);
            }
        }
    }
}
