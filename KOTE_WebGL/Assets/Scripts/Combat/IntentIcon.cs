using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

[RequireComponent(typeof(Image))]
public class IntentIcon : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;
    [SerializeField]
    List<IconMap> iconMap;
    private Image icon;

    [System.Serializable]
    private class IconMap 
    {
        public ENEMY_INTENT type;
        public int valueThreshold;
        public Sprite icon;
    }

    // Start is called before the first frame update
    void Start()
    {
        icon = GetComponent<Image>();
        iconMap.Sort((item1, item2) => item2.valueThreshold - item1.valueThreshold);
        if (text == null) 
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }
        ClearIntent();
        SetIcon(ENEMY_INTENT.attack, 22);
        SetValue(22, 4);
    }

    public void ClearIntent() 
    {
        SetIcon(ENEMY_INTENT.unknown);
        SetValue(0, 0);
    }

    public void SetValue(int value = 0, int times = 1) 
    {
        if (value * times <= 0)
        {
            text.text = "";
        }
        else 
        {
            text.text = $"{value}{(times>1 ? $"x{times}" : "")}";
        }
    }

    public void SetTooltip(string tooltip) 
    {
        
    }

    public void SetIcon(ENEMY_INTENT type, int value = 0) 
    {
        IconMap selected = null;
        foreach (var icon in iconMap) 
        {
            if (icon.type == type) 
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
            Debug.Log($"[IntentIcon] New Icon | {selected.type} - {selected.valueThreshold}");
            icon.sprite = selected.icon;
        }
        else 
        {
            Debug.Log($"[IntentIcon] No Icon Found | {type} - {value}");
        }
    }
}
