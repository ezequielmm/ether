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
        Vector3 anchorPoint = new Vector3(transform.position.x + rectTransform.rect.center.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) / 2),
            transform.position.y + rectTransform.rect.center.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) / 2), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        List<Tooltip> tooltips = new List<Tooltip>() { new Tooltip()
        {
            title = "End Turn",
            description = "Pressing this button will end your turn.\n\nYou will discard your hand, enemies will take their turn, you will draw 5 cards, then it will be your turn again."
        }};
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.BottomRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
