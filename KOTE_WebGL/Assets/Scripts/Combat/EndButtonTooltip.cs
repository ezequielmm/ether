using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    [SerializeField]
    Transform desiredLocation;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(desiredLocation.position.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) * 0.5f),
            desiredLocation.position.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) * 0.5f), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.EndTurnButtonTooltips, TooltipController.Anchor.BottomRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
