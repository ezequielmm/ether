using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEffectManager: MonoBehaviour
{
    [SerializeField]
    GameObject textPrefab;

    [SerializeField]
    public Color textColor = Color.green;

    [SerializeField]
    int maxPoolCount = 10;
    int poolIndex;

    List<GameObject> textPool;

    private GameObject nextText()
    {
        poolIndex++;
        if (poolIndex >= textPool.Count) 
        {
            poolIndex = 0;
        }
        return textPool[poolIndex];
    }

    private void Awake()
    {
        poolIndex = 0;
        textPool = new List<GameObject>();
    }

    void Start()
    {
        for (int i = 0; i < maxPoolCount; i++) 
        {
            var text = Instantiate(textPrefab, this.transform);
            textPool.Add(text);
            text.SetActive(false);
        }
    }

    public void RunAnimation(string text, Color? color) 
    {
        if (color == null) 
        {
            color = textColor;
        }

        // Set Text
        var textObj = nextText();
        TMPro.TextMeshPro tmp = textObj.GetComponent<TMPro.TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color.Value;
        }

        // Apply Animations
        textObj.SetActive(true);
    }
}
