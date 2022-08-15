using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndTurnManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = transform as RectTransform;
    }

    // Update is called once per frame
    public void EndTurn()
    {
        GameManager.Instance.EVENT_END_TURN_CLICKED.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(transform.position.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) * 1f),
            transform.position.y, 0);
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
