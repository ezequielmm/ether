using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class TooltipComponent : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI title;
    [SerializeField]
    TextMeshProUGUI description;


    [SerializeField]
    float fadeSpeed = 0.2f;

    Image background;


    private void Awake()
    {
        title.text = string.Empty;
        description.text = string.Empty;
    }

    private void OnEnable()
    {
        if (background == null) 
        {
            background = GetComponent<Image>();
        }
    }

    public void Populate(Tooltip data) 
    {
        this.title.text = data.title;
        this.description.text = data.description;
    }

    public void Delete()
    {
        transform.SetParent(transform.parent.parent);
        background?.DOFade(0, fadeSpeed);
        title.DOFade(0, fadeSpeed);
        description.DOFade(0, fadeSpeed).OnComplete(() => {
            DOTween.Kill(this.gameObject);
            Destroy(gameObject);
        });
    }
    public void Disable() 
    {
        transform?.SetParent(transform.parent.parent);
        background?.DOFade(0, fadeSpeed);
        title?.DOFade(0, fadeSpeed);
        description?.DOFade(0, fadeSpeed).OnComplete(() => {
            DOTween.Kill(this.gameObject);
            gameObject.SetActive(false);
        });
    }
    public void Enable() 
    {
        DOTween.Kill(this.gameObject);
        if (background != null)
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
        title.color = new Color(title.color.r, title.color.g, title.color.b, 0);
        description.color = new Color(description.color.r, description.color.g, description.color.b, 0);

        background?.DOFade(1, fadeSpeed).SetDelay(0.1f);
        title?.DOFade(1, fadeSpeed).SetDelay(0.1f);
        description?.DOFade(1, fadeSpeed).SetDelay(0.1f);
    }
}
