using UnityEngine;
using UnityEngine.EventSystems;

public class DragableUI : MonoBehaviour, IDragHandler
{
    public GameObject parentObject;

    public void OnDrag(PointerEventData data)
    {
        Debug.Log("console clicked");
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;
        parentObject.transform.position = mousePos;
        Debug.Log(mousePos + " parent position: " + parentObject.transform.position);
    }
}