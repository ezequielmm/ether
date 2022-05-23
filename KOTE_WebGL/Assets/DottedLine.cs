using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class DottedLine : MonoBehaviour
{
    public GameObject test;
    public SpriteShapeController pathController;
    public SpriteShapeController lineController;
    private float _timePassed;

    public int exitNodeId;
    // we need to know the status of the nodes on both ends of the path
    [SerializeField]
    private string entranceNodeStatus;
    [SerializeField]
    private string exitNodeStatus;

    private void Awake()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.AddListener(OnMouseOver);
        test.SetActive(false);
    }

    private void OnMouseOver(int nodeId)
    {
        test.SetActive(nodeId == exitNodeId && entranceNodeStatus == NODE_STATUS.active.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (test.activeSelf)
        {
            _timePassed += Time.deltaTime;
            //float value = Mathf.Lerp(0, 1, Mathf.PingPong(_timePassed, 1));
            float value = Mathf.Lerp(0, 1, _timePassed);

            Vector3 pos = pathController.transform.TransformPoint(GetPoint(pathController.spline, value));
            test.transform.position = pos;
            Vector3 pos2 = test.transform.localPosition;
            pos2.z = -5; //to make it visible over the sprite shape
            test.transform.localPosition = pos2;


            if (value >= 1) _timePassed = 0;
        }
    }

    public void Populate(NodeData exitNode, string entranceNodeStatus)
    {
        this.entranceNodeStatus = entranceNodeStatus;
        this.exitNodeStatus = exitNode.status;
        this.exitNodeId = exitNode.id;
        pathController.spline.SetPosition(4, this.transform.InverseTransformPoint(exitNode.transform.position));
        lineController.spline.SetPosition(4, this.transform.InverseTransformPoint(exitNode.transform.position));
        //TODO: add noise to previous spline points. There are 5 so far
        DetermineIfPathIsShown();
        RelocateSplinePoints();
    }

    private void DetermineIfPathIsShown()
    {
        if (exitNodeStatus == NODE_STATUS.disabled.ToString() || entranceNodeStatus == NODE_STATUS.disabled.ToString())
        {
            lineController.gameObject.SetActive(false);
        }
    }

    private void RelocateSplinePoints()
    {
        float offsetX =
            (pathController.spline.GetPosition(pathController.spline.GetPointCount() - 1).x -
             pathController.spline.GetPosition(0).x) / (pathController.spline.GetPointCount() - 1);
        float offsetY =
            (pathController.spline.GetPosition(pathController.spline.GetPointCount() - 1).y -
             pathController.spline.GetPosition(0).y) / (pathController.spline.GetPointCount() - 1);


        for (int i = 1; i < pathController.spline.GetPointCount(); i++)
        {
            Vector3 pos = pathController.spline.GetPosition(i);
            pos.x = i * offsetX;
            pos.y = i * offsetY;
            pathController.spline.SetPosition(i, pos);
            // set the dotted line to the exact same position
        }
        MatchSplinesToEachOther();
    }

    // match the splines for the path and the dotted line to each other exactly
    private void MatchSplinesToEachOther()
    {
        for (int i = 1; i < pathController.spline.GetPointCount(); i++)
        {
            lineController.spline.SetLeftTangent(i, pathController.spline.GetLeftTangent(i));
            lineController.spline.SetRightTangent(i, pathController.spline.GetRightTangent(i));
            lineController.spline.SetPosition(i, pathController.spline.GetPosition(i));
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