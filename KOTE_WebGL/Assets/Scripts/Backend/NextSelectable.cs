using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NextSelectable : MonoBehaviour
{
    public Selectable nextSelectable;
    private EventSystem system;

    void Start()
    {
        system = EventSystem.current;
    }

    void Update()
    {
        SelectNextSelectable();
    }

    private void SelectNextSelectable()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (system.currentSelectedGameObject == gameObject)
            {
                if (nextSelectable != null)
                {
                    Debug.Log(system.currentSelectedGameObject+"--->"+ nextSelectable.name);

                    InputField inputfield = nextSelectable.GetComponent<InputField>();
                    if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));

                    system.SetSelectedGameObject(nextSelectable.gameObject, new BaseEventData(system));
                    
                }
            }
        }
    }
}