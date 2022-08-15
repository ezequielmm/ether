using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipAtCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipName;
    private List<Tooltip> tooltips = null;
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
            SetTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Tooltip On
        SetTooltip();
        isHoveringOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        isHoveringOver = false;
    }

    private void SetTooltip() 
    {
        List<Tooltip> tempTooltip = tooltips;
        if (tooltips == null) 
        {
            tempTooltip = ToolTipValues.Instance.GetTooltips(tooltipName);
        }
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tempTooltip, anchor, limit, cursor);
    }
}