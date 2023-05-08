using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PointerManager : MonoBehaviour
{
    public GameObject pointerContainer;
    public GameObject pointerTarget;
    public GameObject lineTarget;
    public SpriteShapeController PointerLine;
    
    public Transform targetPoint;
    public Transform originPoint;
    
    [SerializeField]
    public TargetProfile TargetProfile { get; private set; }

    private Spline spline;
    private Vector3[] originalLinePositions; // we need to store the original locations so we can use them as constants
    private Vector3[] originalLeftTangents;
    private Vector3[] originalRightTangents;
    private int splinePointCount; // store this so we don't have to keep grabbing it, since we use it a lot

    public bool overTarget;
    public string targetID;

    bool pointerActive;

    IPointerRunable activePointer;
    private List<IPointerRunable> runables;
    
    private readonly Vector3 plane = new Vector3(1,1,0);

    private void OnEnable()
    {
        if (runables == null) 
        {
            runables = new List<IPointerRunable>();
        }
        runables.Clear();
        var foundRunables = gameObject.GetComponentsInChildren<IPointerRunable>();
        foreach (var runable in foundRunables) 
        {
            runables.Add(runable);
        }
        originPoint.gameObject.SetActive(false);
    }

    private void Start()
    {
        spline = PointerLine.spline;
        splinePointCount = spline.GetPointCount();

        GameManager.Instance.EVENT_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);

        // get the starting points for each point on the spline to use as offsets and constants
        originalLinePositions = new Vector3[splinePointCount];
        originalLeftTangents = new Vector3[splinePointCount];
        originalRightTangents = new Vector3[splinePointCount];
        for (int i = 0; i < splinePointCount; i++)
        {
            originalLinePositions[i] = spline.GetPosition(i);
            originalLeftTangents[i] = spline.GetLeftTangent(i);
            originalRightTangents[i] = spline.GetRightTangent(i);
        }
    }


    public void OnPointerActivated(PointerData data)
    {
        Vector3 pointerOrigin = data.Origin;

        TargetProfile = data.Targets;

        pointerContainer.SetActive(true);
        originPoint.gameObject.SetActive(true);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        //MoveLine(mousePosition, pointerOrigin);

        Vector3 arrowPosition = mousePosition;
        if (mousePosition.x > pointerOrigin.x)
        {
            arrowPosition.x -= 0.2f;
        }

        if (mousePosition.x < pointerOrigin.x)
        {
            arrowPosition.x += 0.2f;
        }

        pointerTarget.transform.position = arrowPosition;
        RotateArrowTowardsMouse(mousePosition, pointerOrigin);
        
        if(lineTarget != null)
            MoveLine(lineTarget.transform.position, pointerOrigin);
        if(originPoint != null)
            originPoint.transform.position = Vector3.Scale(pointerOrigin, plane);
        if(targetPoint != null)
            targetPoint.transform.position = Vector3.Scale(mousePosition, plane);
        
        // Find which runable to use
        activePointer = null;
        foreach (var pointer in runables) 
        {
            if (pointer.PointerType == data.Type) 
            {
                activePointer = pointer;
                break;
            }
        }
        if (activePointer == null) 
        {
            Debug.LogError($"[PointerManager] Can not find run data for pointer type '${data.Type}'.");
            OnPointerDeactivated("");
            return;
        }


        // Play Card Play sound
        if (!pointerActive)
        {
            activePointer.OnSelect();
            pointerActive = true;
        }
    }

    public void OnPointerDeactivated(string originID)
    {
        //if the pointer is over an enemy, play the card
        if (overTarget)
        {
            activePointer.Run(originID, targetID);
        } 
        else if (pointerActive)
        {
            // Play Cancellation sound
            activePointer.OnCancel();
        }

        if (pointerActive)
        {
            pointerActive = false;
        }

        // else return it to the deck
        //GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(id);
        pointerContainer.SetActive(false);
        originPoint.gameObject.SetActive(false);
    }

    private void RotateArrowTowardsMouse(Vector3 mousePosition, Vector3 cardPosition)
    {
        Quaternion rotation = pointerTarget.transform.rotation;
        if (mousePosition.x < cardPosition.x)
        {
            rotation.z = -Mathf.Abs(rotation.z);
        }

        if (mousePosition.x > cardPosition.x)
        {
            rotation.z = Mathf.Abs(rotation.z);
        }

        pointerTarget.transform.rotation = rotation;
    }

    private void MoveLine(Vector3 mousePosition, Vector3 cardPosition)
    {
        // change the world coords to local coords so it's in the right spots
        Vector3 localMousePosition = transform.InverseTransformPoint(mousePosition);
        Vector3 localCardPosition = transform.InverseTransformPoint(cardPosition);

        MoveSplinePoints(localMousePosition, localCardPosition);
        SetSplineTangents(localMousePosition, localCardPosition);
    }

    private void MoveSplinePoints(Vector3 localMousePosition, Vector3 localCardPosition)
    {
        // set the starting point to the card's position
        SetPointerBasePosition(localCardPosition);

        // set the arrow side of the spriteshape to where the pointer is
        SetPointerTipPosition(localMousePosition, localCardPosition);

        // set the rest of the points to their correct location
        for (int i = 1; i < splinePointCount - 1; i++)
        {
            Vector3 newLocation = spline.GetPosition(i);

            //determine what side of the offset it's on
            if (IsToLeftOfCard(localMousePosition, localCardPosition))
            {
                newLocation.x = spline.GetPosition(0).x - originalLinePositions[i].x;
            }
            else if (IsToRightOfCard(localMousePosition, localCardPosition))
            {
                newLocation.x = spline.GetPosition(0).x + originalLinePositions[i].x;
            }

            // if it's on the same side of the card as the arrow point, never let it get within one unit of the arrow
            if (originalLinePositions[i].x > 0)
            {
                if (IsToLeftOfCard(localMousePosition, localCardPosition) &&
                    localMousePosition.x > localCardPosition.x - originalLinePositions[i].x - 1)
                {
                    newLocation.x = localMousePosition.x + 1;
                }
                else if (IsToRightOfCard(localMousePosition, localCardPosition) &&
                         localMousePosition.x < localCardPosition.x + originalLinePositions[i].x + 1)
                {
                    newLocation.x = localMousePosition.x - 1;
                }

                // unless it's within one unit of the card, then it stays at the location of the card
                if (localMousePosition.x > localCardPosition.x - 1 && localMousePosition.x < localCardPosition.x + 1)
                {
                    newLocation.x = localCardPosition.x;
                }
            }

            // if it's on the opposite side of the card from the arrow point, adjust it only when it's in bounds
            if (IsInOffsetBounds(i, localMousePosition) && originalLinePositions[i].x < 0)
            {
                newLocation.x = localCardPosition.x - (localMousePosition.x - localCardPosition.x);
            }

            // set the y value to that of the mouse minus the offset
            float yOffset = originalLinePositions[i].y - originalLinePositions[splinePointCount - 1].y;
            newLocation.y = spline.GetPosition(splinePointCount - 1).y + yOffset;
            // make sure the arrow doesn't go beneath the card
            if (newLocation.y < localCardPosition.y)
            {
                newLocation.y = localCardPosition.y;
            }

            spline.SetPosition(i, newLocation);
        }
    }

    private void SetPointerBasePosition(Vector3 position)
    {
        spline.SetPosition(0, position);
    }

    private void SetPointerTipPosition(Vector3 localMousePosition, Vector3 localCardPosition)
    {
        if (localMousePosition.x < localCardPosition.x)
        {
            localMousePosition.x += 0.2f;
        }
        else
        {
            localMousePosition.x -= 0.2f;
        }

        spline.SetPosition(splinePointCount - 1, localMousePosition);
    }

    private void SetSplineTangents(Vector3 localMousePosition, Vector3 localCardPosition)
    {
        // adjust the tangents of the turn
        for (int i = 0; i < splinePointCount; i++)
        {
            Vector3 leftTangent = spline.GetLeftTangent(i);
            Vector3 rightTangent = spline.GetRightTangent(i);

            if (IsInOffsetBounds(i, localMousePosition))
            {
                // get the offset from the card position as a percentage, and use that to calculate the correct tangent values
                float mouseDistanceFromCardPosition = localMousePosition.x - localCardPosition.x;
                float offsetPercent = mouseDistanceFromCardPosition / originalLinePositions[i].x;
                float leftTangentX = originalLeftTangents[i].x * offsetPercent;
                float rightTangentX = originalRightTangents[i].x * offsetPercent;
                float rightTangentY = originalRightTangents[i].y;

                leftTangent = new Vector3(leftTangentX, originalLeftTangents[i].y, 0);
                rightTangent = new Vector3(rightTangentX, rightTangentY, 0);
            }
            else if (IsToLeftOfCard(localMousePosition, localCardPosition))
            {
                leftTangent = new Vector3(-originalLeftTangents[i].x, originalLeftTangents[i].y, 0);
                rightTangent = new Vector3(-originalRightTangents[i].x, originalRightTangents[i].y, 0);
            }
            else if (IsToRightOfCard(localMousePosition, localCardPosition))
            {
                leftTangent = originalLeftTangents[i];
                rightTangent = originalRightTangents[i];
            }

            // however, if the spline point nearest the pointer is too close, make the right tangent very small
            if ((i == splinePointCount - 2) && IsTooCloseToPointerArrow(i, 2))
            {
                rightTangent = new Vector3(0.01f, 0.01f, 0);
            }

            spline.SetLeftTangent(i, leftTangent);
            spline.SetRightTangent(i, rightTangent);
        }
    }

    private bool IsTooCloseToPointerArrow(int index, float distance)
    {
        return (spline.GetPosition(index).x < spline.GetPosition(splinePointCount - 1).x + distance &&
                spline.GetPosition(index).x > spline.GetPosition(splinePointCount - 1).x - distance) &&
               (spline.GetPosition(index).y < spline.GetPosition(splinePointCount - 1).y + distance &&
                spline.GetPosition(index).y > spline.GetPosition(splinePointCount - 1).y - distance);
    }

    private bool IsInOffsetBounds(int pointIndex, Vector3 localMousePosition)
    {
        bool inRightBounds =
            localMousePosition.x < spline.GetPosition(0).x + Math.Abs(originalLinePositions[pointIndex].x);
        bool inLeftBounds =
            localMousePosition.x > spline.GetPosition(0).x - Math.Abs(originalLinePositions[pointIndex].x);
        return inLeftBounds && inRightBounds;
    }

    private bool IsToLeftOfCard(Vector3 localMousePosition, Vector3 localCardPosition)
    {
        return localMousePosition.x < localCardPosition.x;
    }

    private bool IsToRightOfCard(Vector3 localMousePosition, Vector3 localCardPosition)
    {
        return localMousePosition.x > localCardPosition.x;
    }
}