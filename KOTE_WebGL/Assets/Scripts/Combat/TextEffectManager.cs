using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TextEffectManager: MonoBehaviour
{
    [SerializeField]
    GameObject textPrefab;

    [SerializeField]
    public Color textColor = GameSettings.DEFAUT_TEXTEFFECT_COLOR;

    [SerializeField]
    float riseHeight = GameSettings.DEFAUT_TEXTEFFECT_RISE_HEIGHT;
    [SerializeField]
    Vector2 riseSpeed = GameSettings.DEFAUT_TEXTEFFECT_RISE_SPEED;
    [SerializeField]
    float fadeTime = GameSettings.DEFAUT_TEXTEFFECT_FADE_TIME;
    [SerializeField]
    float xSpread = GameSettings.DEFAUT_TEXTEFFECT_X_SPREAD;

    [SerializeField]
    int maxPoolCount = GameSettings.DEFAUT_TEXTEFFECT_POOL_SIZE;
    int poolIndex;

    [SerializeField]
    string sampleText;
    [SerializeField]
    bool playSampleAnimation;

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

    private void Update()
    {
        if (playSampleAnimation) 
        {
            playSampleAnimation = false;
            RunAnimation(sampleText);
        }
    }

    public void RunAnimation(string text, Color? color = null) 
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

        textObj.transform.localPosition = new Vector3(Random.Range(-xSpread, xSpread), 0, 0);

        StartCoroutine(DoTweenAnimations(textObj.transform, tmp));
    }

    public IEnumerator DoTweenAnimations(Transform obj, TMPro.TextMeshPro tmp) 
    {
        float riseTime = Random.Range(riseSpeed.x, riseSpeed.y);
        obj.localScale = Vector3.zero;
        tmp.alpha = 0;

        obj.DOMoveY(obj.position.y + riseHeight, riseTime);
        obj.DOScale(new Vector2(1.1f, 0.9f), riseTime / 3).SetEase(Ease.Flash).OnComplete(() =>
        {
            obj.DOScale(new Vector2(0.9f, 1.1f), riseTime / 3).SetEase(Ease.Flash).OnComplete(() =>
            {
                obj.DOScale(new Vector2(1.1f, 0.9f), riseTime / 3).SetEase(Ease.Flash);
            });
        });

        tmp.DOFade(textColor.a, 0.5f);
        yield return new WaitForSeconds(riseTime - fadeTime);
        tmp.DOFade(0, fadeTime).OnComplete(() => {
            obj.gameObject.SetActive(false);
        });
    }
}
