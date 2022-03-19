using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    public float rotationSmoothness;

    private Vector3 clickPos;
    public RectTransform pointerRectTransform;

    private bool target;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectTargetable();
        }

        if (Input.GetMouseButtonDown(1))
        {
            StopTargeting();
        }

        if (target)
        {
            StartTargeting();
        }
    }

    public void DetectTargetable()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
            Vector3.forward, Mathf.Infinity);

        if (hit.collider != null && hit.collider.CompareTag("Card"))
        {
            Debug.Log($"card clicked");
            pointerRectTransform.gameObject.SetActive(true);
            clickPos = hit.collider.gameObject.transform.position;
            target = true;
        }
    }

    public void StartTargeting()
    {
        Cursor.visible = false;

        pointerRectTransform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pointerRectTransform.position = new Vector3(pointerRectTransform.position.x, pointerRectTransform.position.y, 1);

        float direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition).x - clickPos.x);

        float angle = -Mathf.Atan(direction) * Mathf.Rad2Deg;
        angle = ApplySmoothness(angle);

        pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private float ApplySmoothness(float angle)
    {
        if (angle == 0)
        {
            return angle;
        }

        float smoothness = Mathf.Abs(angle * (rotationSmoothness / 100));

        if (angle > 0)
        {
            angle -= smoothness;
        }
        else if (angle < 0)
        {
            angle += smoothness;
        }

        return angle;
    }

    public void StopTargeting()
    {
        target = false;
        Cursor.visible = true;
        pointerRectTransform.gameObject.SetActive(false);
    }
}