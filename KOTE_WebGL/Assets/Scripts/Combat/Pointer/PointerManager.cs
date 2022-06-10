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
    private Vector3[] originalLinePositions;
    private int splinePointCount; // store this so we don't have to keep grabbing it, since we use it a lot
    [HideInInspector] public bool overEnemy;

    private void Start()
    {
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
        spline = PointerLine.spline;
        splinePointCount = spline.GetPointCount();

        // get the starting points for each point on the spline to use as offsets
        originalLinePositions = new Vector3[splinePointCount];
        for (int i = 0; i < splinePointCount; i++)
        {
            originalLinePositions[i] = spline.GetPosition(i);
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
        Vector3 localMouseCoord = transform.InverseTransformPoint(mousePosition);
        Vector3 localCardPosition = transform.InverseTransformPoint(cardPosition);

        // set the starting point to the card's position
        spline.SetPosition(0, localCardPosition);

        // set the arrow side of the spriteshape to where the ponter is
        if (localMouseCoord.x < localCardPosition.x)
        {
            localMouseCoord.x += 0.2f;
        }

        if (localMouseCoord.x > localCardPosition.x)
        {
            localMouseCoord.x -= 0.2f;
        }

        spline.SetPosition(splinePointCount - 1, localMouseCoord);

        // adjust the location of the turn
        for (int i = 1; i < splinePointCount - 1; i++)
        {
            Vector3 newLocation = spline.GetPosition(i);
            
            // the points of the spline need to remain in the same x coordinates as in the prefab in relation to the base point
            newLocation.x = spline.GetPosition(0).x + originalLinePositions[i].x;
            
            // but they should be following the y value of the arrow point
            float yOffset = originalLinePositions[i].y - originalLinePositions[splinePointCount - 1].y;
            newLocation.y = spline.GetPosition(splinePointCount - 1).y + yOffset;
            
            spline.SetPosition(i, newLocation);
        }
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