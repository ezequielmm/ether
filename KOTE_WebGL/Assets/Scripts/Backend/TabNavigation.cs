using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
    private EventSystem system;
    void Start()
    {
        system = EventSystem.current;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                TMP_InputField inputField = next.GetComponent<TMP_InputField>();
                if(inputField != null) inputField.OnPointerClick(new PointerEventData(system));
                system.SetSelectedGameObject(next.gameObject);
            }
        }
    }
}
