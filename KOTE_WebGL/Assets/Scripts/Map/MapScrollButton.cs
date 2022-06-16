using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScrollButton : MonoBehaviour
{
    public bool scrollRight;
    private void OnMouseDown()
    {
      
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, scrollRight);       
    }

    private void OnMouseUp()
    {
       // Debug.Log("Mouse up");
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(false,scrollRight);
    }
    private void OnMouseDrag()
    {
        //GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, scrollRight);
    }
}
