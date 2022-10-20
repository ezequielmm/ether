using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BackgroundClickCloser : MonoBehaviour, IPointerClickHandler
{
    public GameObject objectToHide;
    public GraphicRaycaster raycaster;

    // fires another click after hiding the panel so that the input is not consumed
    public void OnPointerClick(PointerEventData data)
    {
        objectToHide.SetActive(false);
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(data, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log("Hit " + result.gameObject.name);
            if (result.gameObject.GetComponent<Selectable>())
            {
                ExecuteEvents.Execute(result.gameObject, data, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
