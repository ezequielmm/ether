using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScrollButton : MonoBehaviour
{
    public bool scrollRight;

    private bool beingHeld;
    private float holdTime = 0;

    private void OnMouseDown()
    {
        Scroll();
        beingHeld = true;
        holdTime = GameSettings.MAP_SCROLL_HOLD_DELAY_DURATION;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && beingHeld)
        {
            if (holdTime > 0)
            {
                holdTime -= Time.deltaTime;
            }
            else 
            {
                Scroll();
            }
        }
        else if (!Input.GetMouseButtonDown(0) && beingHeld) 
        {
            beingHeld = false;
        }
    }

    private void Scroll() 
    {
        GameManager.Instance.EVENT_MAP_SCROLL_CLICK.Invoke(true, scrollRight);
    }
}
