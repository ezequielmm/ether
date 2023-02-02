using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonManager : MonoBehaviour
{
    [SerializeField]
    RectTransform EnabledPoint;
    [SerializeField]
    RectTransform DisabledPoint;

    
    [SerializeField]
    Button ButtonObject;
    RectTransform Button;

    [SerializeField]
    private bool _enabled;

    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            if (value)
            {
                MoveToEnabledPoint();
            }
            else
            {
                MoveToDisabledPoint();
            }
            _enabled = value;
        }
    }

    void Start()
    {
        Button = ButtonObject.GetComponent<RectTransform>();
        Enable();
    }

    public void Enable() 
    {
        ButtonObject.enabled = true;
        Enabled = true;
    }

    public void Disable()
    {
        ButtonObject.enabled = false;
        Enabled = false;
    }

    private void MoveToEnabledPoint()
    {
        Button.DOMove(EnabledPoint.position, 0.5f).SetDelay(1).OnComplete(() => 
        {
            Button.position = EnabledPoint.position;
        });
    }
    private void MoveToDisabledPoint()
    {
        Button.DOMove(DisabledPoint.position, 0.5f).OnComplete(() =>
        {
            Button.position = DisabledPoint.position;
        });
    }
}
