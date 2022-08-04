using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryTabsNavigatorManager : MonoBehaviour
{
    public List<Button> tabButtons;

    [Space(20)] public List<GameObject> panels;

    [Space(20)] public ScrollRect scrollRect;
    
    // I made double implementation because you can't pass two parameters in editor to a button on click event
    public void SelectTab(GameObject selectedPanel)
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(panel == selectedPanel);
        }

        scrollRect.content = selectedPanel.transform as RectTransform;
    }

    public void SelectFirstTab()
    {
        panels[0].SetActive(true);
        tabButtons[0].Select();
    
        for (int i = 1; i < panels.Count; i++)
        {
            panels[i].SetActive(false);
        }

        scrollRect.content = panels[0].transform as RectTransform;
    }

    // these are used so that the scroll buttons control the scrollbar required for the ScrollRect
    public void ScrollUp()
    {
        getMovementOffset();
        scrollRect.verticalScrollbar.value += getMovementOffset();
    }

    public void ScrollDown()
    {
        scrollRect.verticalScrollbar.value -= getMovementOffset();
    }

    private float getMovementOffset()
    {
        int count = scrollRect.content.childCount;
        int rows = count / 5;
        float offset = 1.0f / (rows - 1);
        return offset;
    }
}