using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RightButtonsTooltips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipName;
    RectTransform rectTransform;
    public float heightLimit = 4.3f;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(Screen.width,
            Screen.height + transform.position.y + rectTransform.rect.center.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) / 2), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        if (anchorPoint.y > heightLimit) 
        {
            anchorPoint.y = heightLimit;
        } else if (anchorPoint.y < -heightLimit)
        {
            anchorPoint.y = -heightLimit;
        }
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.GetTooltips(tooltipName), TooltipController.Anchor.TopRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
