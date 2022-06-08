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
    [HideInInspector] public bool overEnemy;

    private void Start()
    {
        GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.AddListener(OnPointerActivated);
        GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.AddListener(OnPointerDeactivated);
        spline = PointerLine.spline;
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
            rotation.x = -Mathf.Abs(rotation.x);
        }

        if (mousePosition.x > cardPosition.x)
        {
            rotation.x = Mathf.Abs(rotation.x);
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
        spline.SetPosition(spline.GetPointCount() - 1, localMouseCoord);
        
       // adjust the location of the turn
        spline.SetPosition(1, new Vector3(localCardPosition.x, localMouseCoord.y, 0));
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