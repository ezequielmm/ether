using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class DottedLine : MonoBehaviour
{
    public GameObject test;
    public SpriteShapeController ssc;
    private float _timePassed;

    public int exitNodeId;
    NODE_STATUS status;

    private void Awake()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.AddListener(OnMouseOver);
        test.SetActive(false);
    }

    private void OnMouseOver(int nodeId)
    {
        test.SetActive(nodeId == exitNodeId && status == NODE_STATUS.active);
    }

    // Update is called once per frame
    void Update()
    {
        if (test.activeSelf)
        {
            _timePassed += Time.deltaTime;
            //float value = Mathf.Lerp(0, 1, Mathf.PingPong(_timePassed, 1));
            float value = Mathf.Lerp(0, 1, _timePassed);

            Vector3 pos = ssc.transform.TransformPoint(GetPoint(ssc.spline, value));
            test.transform.position = pos;
            Vector3 pos2 = test.transform.localPosition;
            pos2.z = -5; //to make it visible over the sprite shape
            test.transform.localPosition = pos2;


            if (value >= 1) _timePassed = 0;
        }
    }

    public void Populate(GameObject targetOb, int eni, NODE_STATUS st)
    {
        status = st;
        exitNodeId = eni;
        ssc.spline.SetPosition(4, this.transform.InverseTransformPoint(targetOb.transform.position));
        //TODO: add noise to previous spline points. There are 5 so far
        RelocateSplinePoints();
    }

    private void RelocateSplinePoints()
    {
        float offsetX = (ssc.spline.GetPosition(ssc.spline.GetPointCount() - 1).x - ssc.spline.GetPosition(0).x) /
                        (ssc.spline.GetPointCount() - 1);
        float offsetY = (ssc.spline.GetPosition(ssc.spline.GetPointCount() - 1).y - ssc.spline.GetPosition(0).y) /
                        (ssc.spline.GetPointCount() - 1);


        for (int i = 1; i < ssc.spline.GetPointCount(); i++)
        {
            Vector3 pos = ssc.spline.GetPosition(i);
            pos.x = i * offsetX;
            pos.y = i * offsetY;
            ssc.spline.SetPosition(i, pos);
        }
    }

    public Vector2 GetPoint(Spline spline, float progress)
    {
        var length = spline.GetPointCount();
        var i = Mathf.Clamp(Mathf.CeilToInt((length - 1) * progress), 0, length - 1);

        var t = progress * (length - 1) % 1f;
        if (i == length - 1 && progress >= 1f)
            t = 1;

        var prevIndex = Mathf.Max(i - 1, 0);

        var _p0 = new Vector2(spline.GetPosition(prevIndex).x, spline.GetPosition(prevIndex).y);
        var _p1 = new Vector2(spline.GetPosition(i).x, spline.GetPosition(i).y);
        var _rt = _p0 + new Vector2(spline.GetRightTangent(prevIndex).x, spline.GetRightTangent(prevIndex).y);
        var _lt = _p1 + new Vector2(spline.GetLeftTangent(i).x, spline.GetLeftTangent(i).y);

        return BezierUtility.BezierPoint(
            new Vector2(_p0.x, _p0.y),
            new Vector2(_rt.x, _rt.y),
            new Vector2(_lt.x, _lt.y),
            new Vector2(_p1.x, _p1.y),
            t
        );
    }
}