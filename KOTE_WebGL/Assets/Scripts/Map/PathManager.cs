using UnityEngine;
using UnityEngine.U2D;

public class PathManager : MonoBehaviour
{
    public SpriteShapeController pathController;
    public SpriteShapeController lineController;
    private float _timePassed;

    public int exitNodeId;

    // we need to know the status of the nodes on both ends of the path
    private string entranceNodeStatus;
    private string exitNodeStatus;

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

    private void Awake()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.AddListener(OnMouseOverExitNode);
    }

    private void OnMouseOverExitNode(int nodeId)
    {
        bool isMousedOver = nodeId == exitNodeId;
        // we have to check if the entrance node is active so we don't deactivate the lines showing the path traveled
        if (entranceNodeStatus == NODE_STATUS.active.ToString())
        {
            lineController.gameObject.SetActive(isMousedOver);
        }
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
}