using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RightButtonsTooltips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<Tooltip> tooltips;
    RectTransform rectTransform;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(Screen.width,
            (Screen.height * -0.03f) + transform.position.y + rectTransform.rect.center.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) / 2), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.TopRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
