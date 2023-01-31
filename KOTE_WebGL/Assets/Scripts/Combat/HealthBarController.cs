using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Sprite HealthBarFill;
    [SerializeField] private Sprite HealthBarBg;
    [SerializeField]
    private List<TransformMap> transformMap;

    [SerializeField]
    private Image background;
    [SerializeField]
    private Image fill;
    [SerializeField]
    private RectTransform fillArea;
    [SerializeField]
    private RectTransform healthBar;

    [SerializeField]
    private Size defaultSize = Size.medium;

    public float widthAdjustment = -39;

    [System.Serializable]
    private class IconMap 
    {
        public Size size;
        public Sprite icon;
    }

    [System.Serializable]
    private class TransformMap
    {
        public Size size;
        public RectTransform transPrefab;
    }

    private void Start()
    {
        SetSize(defaultSize);
    }

    public void SetSize(Size newSize)
    {
        background.sprite = HealthBarBg;
        fill.sprite = HealthBarFill;
        healthBar.sizeDelta = new Vector2(Utils.GetSceneSize(newSize) + widthAdjustment, healthBar.sizeDelta.y);

        var otherTransform = getTransformForSize(transformMap, newSize);
        fillArea.anchoredPosition = otherTransform.anchoredPosition;
        fillArea.sizeDelta = otherTransform.sizeDelta;
        fillArea.anchorMin = otherTransform.anchorMin;
        fillArea.anchorMax = otherTransform.anchorMax;
    }

    private Sprite getImageForSize(List<IconMap> map, Size size) 
    {
        foreach (IconMap item in map) 
        {
            if (item.size == size) 
            {
                return item.icon;
            }
        }
        Debug.LogError($"[HealthBarController] Missing bar art for size {nameof(size)}. Be sure to add it!");
        return null;
    }

    private RectTransform getTransformForSize(List<TransformMap> map, Size size)
    {
        foreach (TransformMap item in map)
        {
            if (item.size == size)
            {
                return item.transPrefab;
            }
        }
        Debug.LogError($"[HealthBarController] Missing transform for size {nameof(size)}. Be sure to add it!");
        return default(RectTransform);
    }
}
