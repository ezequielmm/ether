using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;

public class DefenseController : MonoBehaviour
{
    public Transform desiredPosition;
    public TMP_Text textMeshPro;
    public Mask numberMask;

    public Image defenseIcon;

    public UnityEvent ShieldBroken = new UnityEvent();
    public UnityEvent ShieldRestored = new UnityEvent();

    [SerializeField]
    private Sprite normalShield;
    [SerializeField]
    private Sprite brokenShield;

    private float animationDuration = 0.5f;

    private int _defense = 0;
    bool defenseHidden = false;

    public int Defense { get => _defense; 
        set 
        {
            int oldDef = _defense;
            _defense = value;
            if (value != oldDef)
            {
                UpdateDefenseIcon(value, oldDef);
            }
        } 
    }


    private void UpdateDefenseIcon(int newValue, int oldValue) 
    {
        // Reset container position
        textMeshPro.transform.localPosition = desiredPosition.position;
        DOTween.Kill(textMeshPro.transform);
        
        // Set up variables
        float iconHeight = numberMask.rectTransform.rect.height;
        float runningLength = 0;

        int bias = 1;
        if (newValue < oldValue)
        {
            bias = -1;
        }

        float totalDistance = (newValue - oldValue) * iconHeight;

        var OG = textMeshPro.gameObject;
        // Move original text too
        OG.transform.DOLocalMove(OG.transform.localPosition + Vector3.up * totalDistance, animationDuration).OnComplete(
            () =>
            {
                Destroy(OG);
                CheckSheildStatus();
            }).SetEase(Ease.InOutSine);

        // Spawn text and move it
        for (int i = oldValue + bias; i != newValue + bias; i += bias) 
        {
            runningLength += iconHeight * -bias;
            Vector3 spawnPos = desiredPosition.position;
            spawnPos.y += runningLength;
            var newText = GameObject.Instantiate(textMeshPro.gameObject, spawnPos, desiredPosition.rotation, numberMask.transform);
            var tmp = newText.GetComponent<TMP_Text>();
            // Assign Text
            tmp.text = i.ToString();
            newText.name = i.ToString();
            // Move text
            bool isLast = i == newValue;
            tmp.DOFade(1, animationDuration);
            newText.transform.DOLocalMove(newText.transform.localPosition + Vector3.up * totalDistance, animationDuration).OnComplete(
                ()=> 
                {
                    if (!isLast)
                    {
                        Destroy(newText);
                    }
                }).SetEase(Ease.InOutSine);
            if (isLast) 
            {
                textMeshPro = tmp;
            }
        }
        if (newValue != 0)
        {
            CheckSheildStatus();
        }
    }

    private void SpawnShield() 
    {
        defenseHidden = false;
        defenseIcon.gameObject.SetActive(true);
        defenseIcon.transform.localPosition = Vector3.up * defenseIcon.rectTransform.rect.height;
        defenseIcon.transform.DOLocalMove(Vector3.zero, animationDuration);
        defenseIcon.color = new Color(1, 1, 1, 0);
        defenseIcon.DOFade(1, animationDuration);

        defenseIcon.sprite = normalShield;

        textMeshPro.color = new Color(1,1,1, 0);
        textMeshPro.DOFade(1, animationDuration);

        ShieldRestored.Invoke();
    }

    private void BreakSheild() 
    {
        defenseHidden = true;
        defenseIcon.sprite = brokenShield;
        defenseIcon.transform.DOLocalMove(Vector3.down * defenseIcon.rectTransform.rect.height, animationDuration);
        defenseIcon.DOFade(0, animationDuration).OnComplete(() => 
        {
            defenseIcon.sprite = normalShield;
            defenseIcon.gameObject.SetActive(false);
        });

        textMeshPro.DOFade(0, animationDuration);
        ShieldBroken.Invoke();
    }

    private void CheckSheildStatus() 
    {
        if (defenseHidden && Defense > 0)
        {
            SpawnShield();
        }
        else if (!defenseHidden && Defense <= 0)
        {
            BreakSheild();
        }
    }

    void Start()
    {
        textMeshPro.text = Defense.ToString();
        textMeshPro.name = Defense.ToString();
        if (Defense == 0) 
        {
            defenseHidden = true;
            defenseIcon.gameObject.SetActive(false);
            ShieldBroken.Invoke();
        }
    }
}
