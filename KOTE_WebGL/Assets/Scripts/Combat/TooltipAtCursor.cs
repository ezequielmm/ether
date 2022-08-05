using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipAtCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<Tooltip> tooltips;
    public TooltipController.Anchor anchor;
    public Vector2 limit = -Vector2.one;

    bool isHoveringOver;

    Transform cursor;

    private void Start()
    {
        cursor = Cursor.instance.transform;
        isHoveringOver = false;
    }

    public void SetTooltips(List<Tooltip> newTooltips) 
    {
        tooltips = newTooltips;
        if (isHoveringOver) 
        {
            GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, anchor, limit, cursor);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, anchor, limit, cursor);
        isHoveringOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        isHoveringOver = false;
    }
}