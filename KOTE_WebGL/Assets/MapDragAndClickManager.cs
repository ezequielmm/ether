using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDragAndClickManager : MonoBehaviour
{
    public GameObject nodesHolder;
    float lastTimeClick;
    private Vector3 dragOffset = Vector3.zero;

    private void OnMouseDown()
    {
        if (lastTimeClick > 0)
        {
            if (Time.fixedTime - lastTimeClick < GameSettings.DOUBLE_CLICK_TIME_DELTA)
            {
                Debug.Log("Double click!");
                lastTimeClick = 0;
                GameManager.Instance.EVENT_MAP_MASK_DOUBLECLICK.Invoke();
            }
            else
            {
                Debug.Log("click!");
                lastTimeClick = Time.fixedTime;
            }
        }
        else
        {
            Debug.Log("First click!");
            lastTimeClick = Time.fixedTime;
        }

        dragOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - nodesHolder.transform.position;
    }


    private void OnMouseDrag()
    {
        GameManager.Instance.EVENT_MAP_SCROLL_DRAG.Invoke(dragOffset);
    }

    private void OnMouseUp()
    {
        dragOffset = Vector3.zero;
    }
}