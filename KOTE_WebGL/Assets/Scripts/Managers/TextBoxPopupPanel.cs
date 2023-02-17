using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBoxPopupPanel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text textBox;
    [SerializeField]
    private GameObject rootObject;

    void Start()
    {
        Disable();
        SetContent(string.Empty);
    }

    public void Enable() 
    {
        SetEnabled(true);
    }
    public void Disable()
    {
        SetEnabled(false);
    }
    public void SetEnabled(bool active)
    {
        rootObject.SetActive(active);
    }
    public void ToggleEnabled()
    {
        SetEnabled(!rootObject.activeSelf);
    }

    public void SetContent(string text) 
    {
        textBox.text = text;
    }

    public void Popup(string content) 
    {
        SetContent(content);
        Enable();
    }
}
