using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.U2D;

public class PointerManager : MonoBehaviour
{
    public GameObject pointerContainer;
    public GameObject pointerTarget;
    public SpriteShapeController PointerLine;
    private Spline spline;
    private Vector3[] originalLinePositions; // we need to store the original locations so we can use them as constants
    private Vector3[] originalLeftTangents;
    private Vector3[] originalRightTangents;
    private int splinePointCount; // store this so we don't have to keep grabbing it, since we use it a lot
    [HideInInspector] public bool overEnemy;

    private void Start()
    {
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
        spline = PointerLine.spline;
        splinePointCount = spline.GetPointCount();

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


    private void OnPointerActivated(Vector3 cardPosition)
    {
        pointerContainer.SetActive(true);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        MoveLine(mousePosition, cardPosition);

        float distance = 0.2f;
        Vector3 arrowPosition = mousePosition;
        if (mousePosition.x > cardPosition.x)
        {
            arrowPosition.x -= 0.2f;
        }

        if (mousePosition.x < cardPosition.x)
        {
            arrowPosition.x += 0.2f;
        }

        pointerTarget.transform.position = arrowPosition;
        RotateArrowTowardsMouse(mousePosition, cardPosition);

        //TODO get the tip of the arrow to scale larger as it gets further away from the card
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
        spline.SetPosition(0, localCardPosition);

        // set the arrow side of the spriteshape to where the ponter is
        if (localMousePosition.x < localCardPosition.x)
        {
            localMousePosition.x += 0.2f;
        }
        else
        {
            localMousePosition.x -= 0.2f;
        }

        spline.SetPosition(splinePointCount - 1, localMousePosition);

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

            // however, if it's in the bounds of the offset, adjust it with the position of the mouse

            // if it's on the opposite side of the card from the arrow point, adjust it only when it's in bounds
            if (IsInOffsetBounds(i, localMousePosition))
            {
                if (originalLinePositions[i].x < 0)
                    newLocation.x = localCardPosition.x - (localMousePosition.x - localCardPosition.x);
                else newLocation.x = localMousePosition.x;
            }
            
            // set the y value to that of the mouse minus the offset
            float yOffset = originalLinePositions[i].y - originalLinePositions[splinePointCount - 1].y;
            newLocation.y = spline.GetPosition(splinePointCount - 1).y + yOffset;

            spline.SetPosition(i, newLocation);
        }
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

                leftTangent = new Vector3(leftTangentX, originalLeftTangents[i].y, 0);
                rightTangent = new Vector3(rightTangentX, originalRightTangents[i].y, 0);
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

            spline.SetLeftTangent(i, leftTangent);
            spline.SetRightTangent(i, rightTangent);
        }
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


    private void OnPointerDeactivated(string id)
    {
        pointerContainer.SetActive(false);
        //if the pointer is over an enemy, play the card
        if (overEnemy)
        {
            GameManager.Instance.EVENT_CARD_PLAYED.Invoke(id);
        }

        // else return it to the deck
        //GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(id);
    }
}