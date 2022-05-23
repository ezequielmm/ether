using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDragAndClickManager : MonoBehaviour
{
    float lastTimeClick = 0;
    float previousDragX = 0;

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
    }



    private void OnMouseDrag()
    {
        if (Input.mousePosition.x == previousDragX) return;

        if (previousDragX != 0)
        {
            float diff = Input.mousePosition.x - previousDragX;


            if (diff > 1)
            {
                //Debug.Log("drag RRRRRRRRRR!" + diff);

                GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, false);
            }
            else if(diff < -1)
            {
               // Debug.Log("drag LLLLLL!" + diff);
                GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, true);
            }           
           
        }

        previousDragX = Input.mousePosition.x;

    }

    private void OnMouseUp()
    {
        previousDragX = 0;
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(false, false);
    }
}

