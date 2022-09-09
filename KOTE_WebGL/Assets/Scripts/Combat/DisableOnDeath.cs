using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DisableOnDeath : MonoBehaviour
{
    [Tooltip("Upon death, this object will shrink before destroying itself.")]
    public bool ShrinkToDie = true;

    [Tooltip("The time it takes to run an animation before destroying itself.")]
    public float animationTime = 1f;

    [Tooltip("Unparents itself upon death message.")]
    public bool UnParent = false;
    private void Awake()
    {
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(onGameChange);
    }

    void onGameChange(GameStatuses newState) 
    {
        if (newState == GameStatuses.GameOver) 
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
