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

    [SerializeField]
    bool active;

    RectTransform rectTransform;

    List<TooltipComponent> activeTooltips;
    Transform toFollow;

    bool offsetSet;
    Vector3 offset;
    Vector3 location;
    Anchor anchor;
    bool followPoint;
    Vector2 size;
    Vector3 limit;

    void Start()
    {
        limit = -Vector3.one;
        active = true;
        rectTransform = tooltipContainer.GetComponent<RectTransform>();
        followPoint = false;
        activeTooltips = new List<TooltipComponent>();
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener(ClearTooltips);
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener(SetTooltips);
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
    }

    public void OnPointerActivated(Vector3 data) 
    {
        active = false;
        ClearTooltips();
    }
    public void OnPointerDeactivated(string data) 
    {
        active = true;
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
        if (!active) return;
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
            followPoint = true;
            if(location != Vector3.zero)
                limit = location;
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
        if (!active) return;
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
            var followLocation = toFollow.position;
            followLocation.z = 0;
            if (limit.x != -1) 
            {
                followLocation.x = limit.x;
            }
            if (limit.y != -1) 
            {
                followLocation.y = limit.y;
            }

            followLocation = Camera.main.WorldToScreenPoint(followLocation);

            //Debug.Log($"[{gameObject.name}] toFollow: {toFollow.position} | screenSpace: {followLocation} | offset: {offset}");

            tooltipContainer.transform.position = followLocation + offset;
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
