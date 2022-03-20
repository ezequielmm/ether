using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    public float cursorRotationSmoothness;

    private GameObject sourceGO;
    public Vector3 baseVector;
    public Transform pointerTransform;

    private bool target;

    [Space(20)]
    public int bodyAmount;

    public GameObject bodyPrefab;

    private Vector2 bezierPoint0;
    private Vector2 bezierPoint1;
    private Vector2 bezierPoint2;

    private void Start()
    {
        GameManager.Instance.EVENT_START_POINTER.AddListener(SetTargeting);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ChooseTarget();
        }

        if (Input.GetMouseButtonDown(1))
        {
            StopTargeting();
        }

        if (target)
        {
            StartTargeting();
            UpdateBody();
        }
    }

    public void SetTargeting(GameObject invoker)
    {
        if (sourceGO != null) return;

        pointerTransform.gameObject.SetActive(true);
        sourceGO = invoker;
        target = true;
        CreateArrowBody();
    }

    public void StartTargeting()
    {
        Cursor.visible = false;

        pointerTransform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pointerTransform.position = new Vector3(pointerTransform.position.x, pointerTransform.position.y, 1);

        float direction = pointerTransform.position.x - baseVector.x;

        float angle = -Mathf.Atan(direction) * Mathf.Rad2Deg;
        angle = ApplySmoothness(angle, cursorRotationSmoothness);

        pointerTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void CreateArrowBody()
    {
        DestroyArrowBody();

        for (int i = 0; i < bodyAmount; i++)
        {
            GameObject currentBody = Instantiate(bodyPrefab, transform);
            currentBody.transform.position = new Vector3(0, pointerTransform.position.y);
        }
    }

    public void UpdateBody()
    {
        bezierPoint0 = baseVector;
        bezierPoint2 = pointerTransform.position;
        bezierPoint1 = new Vector2(0, bezierPoint0.y + (bezierPoint2.y - bezierPoint0.y) * 0.80f);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject == pointerTransform.gameObject) continue;

            float floatI = i;
            //position
            Vector3 bezierPoint = CalculateQuadraticBezierPoint((transform.childCount - floatI) / transform.childCount, bezierPoint0, bezierPoint1, bezierPoint2);

            transform.GetChild(i).transform.position = bezierPoint;

            //rotation
            float direction = pointerTransform.position.x - baseVector.x;

            float angle = -Mathf.Atan(direction) * Mathf.Rad2Deg;
            transform.GetChild(i).transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Clamp((100f - 10f * i) / 100f, 0, 100));
        }
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }

    private float ApplySmoothness(float angle, float rotationSmoothness)
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

    private void StopTargeting()
    {
        target = false;
        sourceGO = null;
        Cursor.visible = true;
        pointerTransform.gameObject.SetActive(false);

        DestroyArrowBody();
    }

    private void DestroyArrowBody()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i) == pointerTransform) continue;

            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void ChooseTarget()
    {
        if (target)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                Vector3.forward, Mathf.Infinity);

            if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Player")))
            {
                GameManager.Instance.EVENT_TARGET_POINTER_SELECTED.Invoke(sourceGO);
                StopTargeting();
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(baseVector, 0.1f);
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(bezierPoint0, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(bezierPoint1, 0.1f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(bezierPoint2, 0.1f);
    }
}