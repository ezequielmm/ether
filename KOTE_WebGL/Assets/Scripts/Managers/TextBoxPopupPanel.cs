using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxPopupPanel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_Text textBox;
    [SerializeField]
    private GameObject rootObject;
    [SerializeField]
    private VerticalLayoutGroup layoutGroup;

    void Start()
    {
        SetContent(string.Empty);
        Disable();
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
        if (active) 
        {
            RefreshContainer();
        }
    }
    public void ToggleEnabled()
    {
        SetEnabled(!rootObject.activeSelf);
    }

    public void SetContent(string text) 
    {
        textBox.text = text;
        RefreshContainer();
    }

    public void Popup(string content) 
    {
        Enable();
        SetContent(content);
    }

    private void RefreshContainer() 
    {
        Canvas.ForceUpdateCanvases();
        layoutGroup.enabled = false;
        layoutGroup.enabled = true;
    }
}
