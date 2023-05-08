using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class NodeView : MonoBehaviour
{
    [SerializeField] private UnityEvent _onEnabledNodeHover;
    [SerializeField] private UnityEvent _onEnabledNodeExit;
    [SerializeField] private Material grayscaleMaterial;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _originalScale;
    private Tween activeAnimation;

    private bool hoverEntered;
    private NODE_STATUS _status;

    public void Init(NODE_STATUS status)
    {
        _status = status;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }
    
    public void SetResize()
    {
        if (_status == NODE_STATUS.disabled || _status == NODE_STATUS.completed)
        {
            DisabledOrCompleted();
        }
        else if (_status == NODE_STATUS.active || _status == NODE_STATUS.available)
        {
            ActiveOrAvailable();
        }
    }

    public void ActiveOrAvailable()
    {
        //_onNodeEnabled?.Invoke();
        //gameObject.transform.localScale *= 1.2f;
        PlayActiveNodeAnimation();
    }
    
    public void DisabledOrCompleted()
    {
        _onEnabledNodeExit?.Invoke();
        gameObject.transform.localScale *= GameSettings.COMPLETED_NODE_SCALE;
        if (GameSettings.COLOR_UNAVAILABLE_MAP_NODES == false)
        {
            _spriteRenderer.material = grayscaleMaterial;
        }
    }
    
    private void PlayActiveNodeAnimation()
    {
        activeAnimation = transform.DOScale(transform.localScale * 0.7f,
                GameSettings.ACTIVE_NODE_PULSE_TIME)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Stop()
    {
        activeAnimation.Kill();
        transform.localScale = _originalScale;
    }

    public void Rewind()
    {
        activeAnimation.Rewind();
    }

    public void OnDestroy()
    {
        DOTween.Kill(transform);
    }

    public void OnHover()
    {
        if (!(_status == NODE_STATUS.available || _status == NODE_STATUS.active))
            return;

        if (!hoverEntered)
        {
            _onEnabledNodeHover?.Invoke();
        }
        hoverEntered = true;
        //transform.DOScale(new Vector3(_originalScale.x * 1.2f, _originalScale.y * 1.2f), 0.5f);
    }

    public void OnUnHover()
    {
        if (!(_status == NODE_STATUS.available || _status == NODE_STATUS.active))
            return;
        
        if (hoverEntered)
            _onEnabledNodeExit?.Invoke();
        
        hoverEntered = false;
        //transform.DOScale(_originalScale, 0.5f);
    }
}
