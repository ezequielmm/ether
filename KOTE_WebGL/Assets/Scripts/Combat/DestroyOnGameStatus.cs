using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestroyOnGameStatus : MonoBehaviour
{
    [Tooltip("Upon message, this object will shrink before destroying itself.")]
    public bool ShrinkToDie = true;
    [Tooltip("Upon message, this object will move by the provided position.")]
    public bool MoveToDie = false;
    [Tooltip("When true, the move will move by the item's width")]
    public bool MoveMultipleOfSelf = false;
    public Vector3 MoveBy;

    [Tooltip("The time it takes to run an animation before destroying itself.")]
    public float animationTime = 1f;

    [Tooltip("Unparents itself upon message.")]
    public bool UnParent = false;

    public GameStatuses statusToListenTo = GameStatuses.GameOver;
    private void Awake()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(onGameChange);
    }

    void onGameChange(GameStatuses newState) 
    {
        if (newState == statusToListenTo) 
        {
            if (UnParent) 
            {
                transform.parent = null;
            }
            StartCoroutine(DestroySelf());
        }
    }

    IEnumerator DestroySelf() 
    {
        if (ShrinkToDie) 
        {
            transform.DOScale(Vector3.zero, animationTime);
        }

        if (MoveToDie) 
        {
            Vector3 moveAmount = MoveBy;
            if (MoveMultipleOfSelf) 
            {
                var rectTrans = GetComponent<RectTransform>();
                if (rectTrans != null) 
                {
                    moveAmount = new Vector3(MoveBy.x * rectTrans.rect.width, MoveBy.y * rectTrans.rect.height, MoveBy.z * 0);
                }
            }
            transform.DOMove(transform.position + moveAmount, animationTime);
        }

        if (animationTime <= 0) 
        {
            Destroy(gameObject);
        }

        yield return new WaitForSeconds(animationTime);

        if (animationTime > 0f)
        {
            DOTween.Kill(transform);
            Destroy(gameObject);
        }
    }
}
