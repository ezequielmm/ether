using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public class PathManager : MonoBehaviour
{
    public SpriteShapeController pathController;
    public SpriteShapeController lineController;
    private float _timePassed;

    public int exitNodeId;

    // store the step and act so we can animate the map expansion
    private int pathStep;
    private int pathAct;

    // we need to know the status of the nodes on both ends of the path
    private NODE_STATUS entranceNodeStatus;
    private NODE_STATUS exitNodeStatus;

    // stored so we can activate the node when the animation reaches it
    private NodeData exitNode;

    // save the final positions of the points of the spline for animation purposes
    private Vector3[] splinePointPositions;

    // bool to make sure the path is only animated once
    private bool pathAnimated;

    public void Populate(NodeData exitNode, NODE_STATUS entranceNodeStatus)
    {
        this.entranceNodeStatus = entranceNodeStatus;
        this.exitNode = exitNode;
        this.exitNodeStatus = exitNode.status;
        this.exitNodeId = exitNode.id;
        pathStep = exitNode.step;
        pathAct = exitNode.act;
        pathController.spline.SetPosition(4, this.transform.InverseTransformPoint(exitNode.transform.position));
        lineController.spline.SetPosition(4, this.transform.InverseTransformPoint(exitNode.transform.position));
        //TODO: add noise to previous spline points. There are 5 so far
        DetermineIfPathIsShown();
        RelocateSplinePoints();
    }

    private void Awake()
    {
        GameManager.Instance.EVENT_MAP_NODE_MOUSE_OVER.AddListener(OnMouseOverExitNode);
        GameManager.Instance.EVENT_MAP_ANIMATE_STEP.AddListener(AnimateSplinePoints);
    }

    private void OnMouseOverExitNode(int nodeId)
    {
        // check if the moused over node is the exit node, and it's available
        bool isMousedOver = nodeId == exitNodeId && exitNodeStatus == NODE_STATUS.available;
        // we have to check if the entrance node is active so we don't deactivate the lines showing the path traveled
        if (entranceNodeStatus == NODE_STATUS.active)
        {
            lineController.gameObject.SetActive(isMousedOver);
        }
    }

    private void DetermineIfPathIsShown()
    {
        if (exitNodeStatus == NODE_STATUS.disabled || entranceNodeStatus == NODE_STATUS.disabled ||
            exitNodeStatus == NODE_STATUS.available)
        {
            lineController.gameObject.SetActive(false);
        }
    }

    private void AnimateSplinePoints(int act, int step)
    {
        // hide the path if it hasn't been animated yet and it's in the correct act
        if (pathAnimated == false && pathAct == act) pathController.gameObject.SetActive(false);
        if (pathAnimated == false && pathAct == act && pathStep == step)
        {
            pathAnimated = true;
            // move all the points to the beginning node
            float offset = 0.1f;
            Vector3 startingPoint = pathController.spline.GetPosition(0);
            for (int i = 0; i < pathController.spline.GetPointCount(); i++)
            {
                pathController.spline.SetPosition(i,
                    new Vector3(startingPoint.x + offset, startingPoint.y, startingPoint.z));
                offset += 0.1f;
            }

            // then show the path and start animating
            pathController.gameObject.SetActive(true);
            StartCoroutine(AnimateSpline());
        }
    }

    private IEnumerator AnimateSpline()
    {
        bool pathInCorrectPosition = false;
        // animate the paths extending from the nodes
        while (!pathInCorrectPosition)
        {
            // we don't need to update point 0
            for (int i = pathController.spline.GetPointCount() - 1; i > 0; i--)
            {
                if (Math.Abs(pathController.spline.GetPosition(i).x) >= Math.Abs(splinePointPositions[i].x) &&
                    Math.Abs(pathController.spline.GetPosition(i).y) >= Math.Abs(splinePointPositions[i].y))
                {
                    pathInCorrectPosition = true;
                    pathController.spline.SetPosition(i, splinePointPositions[i]);
                    continue;
                }

                Vector3 pointPos = pathController.spline.GetPosition(i);

                // move the path along the line defined by the starting point and the ending point
                // we need to get it in the form y = m * x + b
                // slope = m
                float slope = (splinePointPositions[i].y - pathController.spline.GetPosition(0).y) /
                              (splinePointPositions[i].x - pathController.spline.GetPosition(0).x);
                // yintercept = b
                float yIntercept = splinePointPositions[i].y - (slope * splinePointPositions[i].x);
                
                // add the speed we want the paths to animate to the right at
                if (splinePointPositions[i].x > 0) pointPos.x += GameSettings.MAP_REVEAL_ANIMATION_SPEED;
                if (splinePointPositions[i].x < 0) pointPos.x -= GameSettings.MAP_REVEAL_ANIMATION_SPEED;
                
                // and get the position of y by using y= m * x + b
                float yPosition = (slope * pointPos.x) + yIntercept;
                pointPos.y = yPosition;

                pathController.spline.SetPosition(i, pointPos);
                pathInCorrectPosition = false;
            }
            
            // check to make sure that the final point is at the next node before continuing
            if (pathController.spline.GetPosition(pathController.spline.GetPointCount() - 1) !=
                splinePointPositions[pathController.spline.GetPointCount() - 1])
            {
                pathInCorrectPosition = false;
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }

        // make the nodes appear when the path reaches them
        exitNode.ShowNode();
        // and animate the next step
        GameManager.Instance.EVENT_MAP_ANIMATE_STEP.Invoke(pathAct, pathStep + 1);
    }

    private void RelocateSplinePoints()
    {
        float offsetX =
            (pathController.spline.GetPosition(pathController.spline.GetPointCount() - 1).x -
             pathController.spline.GetPosition(0).x) / (pathController.spline.GetPointCount() - 1);
        float offsetY =
            (pathController.spline.GetPosition(pathController.spline.GetPointCount() - 1).y -
             pathController.spline.GetPosition(0).y) / (pathController.spline.GetPointCount() - 1);

        // save the spline point positions so we know the final position of the spline
        splinePointPositions = new Vector3[pathController.spline.GetPointCount()];
        splinePointPositions[0] = pathController.spline.GetPosition(0);

        for (int i = 1; i < pathController.spline.GetPointCount(); i++)
        {
            Vector3 pos = pathController.spline.GetPosition(i);
            pos.x = i * offsetX;
            pos.y = i * offsetY;
            pathController.spline.SetPosition(i, pos);
            splinePointPositions[i] = pathController.spline.GetPosition(i);
        }

        // set the dotted line to the exact same position
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