using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Collider2D))]
public class IconMap<T> : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The icon text field")]
    private TextMeshProUGUI text;

    [Tooltip("A map between a type and it's related icon")]
    [SerializeField]
    private List<Icon> iconMap;

    [SerializeField]
    [Tooltip("The gameobject holding all the tooltip stuff")]
    private GameObject tooltipContainer;

    [SerializeField]
    [Tooltip("The tooltip's text")]
    private TextMeshProUGUI description;

    [SerializeField]
    [Tooltip("The speed at which the tooltip appears and fades")]
    protected float tooltipSpeed = 1f;

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
        tooltipContainer.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (!string.IsNullOrEmpty(description.text))
        {
            tooltipContainer.SetActive(true);
            tooltipContainer.transform.localScale = Vector3.zero;
            tooltipContainer.transform.DOScale(Vector3.one, tooltipSpeed);
        }
    }
    private void OnMouseExit()
    {
        tooltipContainer.transform.DOScale(Vector3.zero, tooltipSpeed);
    }

    public void Initialize()
    {
        Start();
    }

    public void SetTooltip(string tooltip)
    {
        description.text = tooltip;
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
