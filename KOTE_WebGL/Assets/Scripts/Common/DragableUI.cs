using UnityEngine;

public class DragableUI : MonoBehaviour
{
    public GameObject parentObject;
    private void OnMouseDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
       parentObject.transform.position = mousePos;
    }
}