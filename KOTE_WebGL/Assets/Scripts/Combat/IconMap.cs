using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconMap<T> : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The icon text field")]
    private TextMeshProUGUI text;

    [Tooltip("A map between a type and it's related icon")]
    [SerializeField]
    private List<Icon> iconMap;

    [SerializeField]
    [Tooltip("The speed at which the tooltip appears and fades")]
    protected float tooltipSpeed = 1f;

    private string tooltipTitle;
    private string tooltipDescription;


    /// <summary>
    /// The image component
    /// </summary>
    private Image icon;


    [System.Serializable]
    protected class Icon
    {
        public T type;
        public int valueThreshold;
        public Sprite icon;
    }

    private void Start()
    {
        if(icon == null)
            icon = GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogError($"[{gameObject.name}] Image component could not be found.");
        }
        iconMap.Sort((item1, item2) => item2.valueThreshold - item1.valueThreshold);
        if (text == null)
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Initialize()
    {
        Start();
    }

    public void SetTooltip(string title, string description)
    {
        tooltipTitle = title;
        tooltipDescription = description;
    }

    public Tooltip GetTooltip() 
    {
        return new Tooltip()
        {
            title = Utils.PrettyText(tooltipTitle).Replace(" Plus", "+"),
            description = tooltipDescription
        };
    }

    public void SetDisplayText(string value)
    {
        text.text = value;
    }

    public void SetIcon(T type, int value = 0)
    {
        gameObject.name = type.ToString();
        Icon selected = null;
        foreach (var icon in iconMap)
        {
            if (icon.type.Equals(type))
            {
                if (value >= icon.valueThreshold)
                {
                    selected = icon;
                    break;
                }
            }
        }

        if (selected != null)
        {
            Debug.Log($"[{gameObject.name}] New Icon | {selected.type} - {selected.valueThreshold}");
            icon.sprite = selected.icon;
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No Icon Found | {type} - {value}");
        }
    }
}
