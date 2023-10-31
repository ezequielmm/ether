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
    public int pathStep {get; private set;}
    public int pathAct {get; private set;}

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
        pathController.spline.SetPosition(pathController.spline.GetPointCount() - 1, this.transform.InverseTransformPoint(exitNode.transform.position));
        lineController.spline.SetPosition(lineController.spline.GetPointCount() - 1, this.transform.InverseTransformPoint(exitNode.transform.position));
        DetermineIfPathIsShown();
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
}