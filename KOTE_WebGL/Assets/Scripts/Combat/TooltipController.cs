using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    [SerializeField]
    GameObject tooltipPrefab;

    [SerializeField]
    GameObject tooltipContainer;

    RectTransform rectTransform;

    List<TooltipComponent> activeTooltips;
    Transform toFollow;

    bool offsetSet;
    Vector3 offset;
    Vector3 location;
    Anchor anchor;
    bool followPoint;
    Vector2 size;

    void Start()
    {
        rectTransform = tooltipContainer.GetComponent<RectTransform>();
        followPoint = false;
        activeTooltips = new List<TooltipComponent>();
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener(ClearTooltips);
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener(SetTooltips);
    }

    public void ClearTooltips() 
    {
        foreach (TooltipComponent tooltip in activeTooltips)
        {
            tooltip.Delete();
        }
        activeTooltips.Clear();
        followPoint = false;
        offset = Vector3.zero;
    }

    public void SetTooltips(List<Tooltip> tooltips, Anchor position, Vector3 location, Transform follow)
    {
        ClearTooltips();
        foreach (Tooltip data in tooltips) {
            var tooltipObj = Instantiate(tooltipPrefab, tooltipContainer.transform);
            var tooltip = tooltipObj.GetComponent<TooltipComponent>();
            tooltip.Enable();
            tooltip.Populate(data);
            activeTooltips.Add(tooltip);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        offsetSet = false;
        if (follow != null)
        {
            toFollow = follow;
        }
        this.location = location;
        anchor = position;
    }

    public void SetLocation(Anchor position, Vector3 location, Transform follow) 
    {
        location.z = 0;
        location = Camera.main.WorldToScreenPoint(location);
        
        string pos = System.Enum.GetName(typeof(Anchor), position).ToLower();
        if (pos.Contains("top"))
        {
            offset.y = -rectTransform.rect.height / 2;
        } 
        else if (pos.Contains("bottom"))
        {
            offset.y = rectTransform.rect.height / 2;
        }

        if (pos.Contains("left"))
        {
            offset.x = rectTransform.rect.width / 2;
        }
        else if (pos.Contains("right"))
        {
            offset.x = -rectTransform.rect.width / 2;
        }

        offset.x *= tooltipContainer.transform.lossyScale.x;
        offset.y *= tooltipContainer.transform.lossyScale.y;

        //Debug.Log($"[TooltipController] Size ({rectTransform.rect.width}, {rectTransform.rect.height}) | offset ({offset.x}, {offset.y})");

        location.z = 0;
        if (!followPoint)
            tooltipContainer.transform.position = location + offset;

        offsetSet = true;
    }

    private void Update()
    {
        Vector2 newSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        if (newSize != size) 
        {
            offsetSet = false;
            size = newSize;
        }
        if (!offsetSet) 
        {
            SetLocation(anchor, location, toFollow);
        }
        if (followPoint) 
        {
            var location = Camera.main.WorldToScreenPoint(toFollow.position);
            location.z = 0;
            tooltipContainer.transform.position = location + offset;
        }
    }

    public enum Anchor 
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
