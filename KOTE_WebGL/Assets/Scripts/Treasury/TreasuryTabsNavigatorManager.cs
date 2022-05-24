using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasuryTabsNavigatorManager : MonoBehaviour
{
    public List<Image> tabButtons;

    [Space(20)] public List<GameObject> panels;

    [Space(20)] public ScrollRect scrollRect;

    [Space(20)] public Color selectedColor;

    public Color nonSelectedColor;

    // I made double implementation because you can't pass two parameters in editor to a button on click event
    public void SelectTab(TabNavigatorInfo navigatorInfo)
    {
        foreach (var button in tabButtons)
        {
            button.color = button == navigatorInfo.selectedButton ? selectedColor : nonSelectedColor;
        }

        foreach (var panel in panels)
        {
            panel.SetActive(panel == navigatorInfo.selectedPanel);
        }

        scrollRect.content = navigatorInfo.selectedPanel.transform as RectTransform;
    }

    public void SelectTab(Image selectedButton, GameObject selectedPanel)
    {
        foreach (var button in tabButtons)
        {
            button.color = button == selectedButton ? selectedColor : nonSelectedColor;
        }

        foreach (var panel in panels)
        {
            panel.SetActive(panel == selectedPanel);
        }

        scrollRect.content = selectedPanel.transform as RectTransform;
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