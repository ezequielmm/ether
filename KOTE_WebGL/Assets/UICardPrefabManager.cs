using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UICardPrefabManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI energyTF;
    public TextMeshProUGUI typeTF;
    public TextMeshProUGUI rarityTF;
    public TextMeshProUGUI descriptionTF;
    public string id;

    private Card card;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void populate(Card card)
    {
        energyTF.SetText(card.energy.ToString());
        typeTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {      
        DOTween.PlayForward(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {      
        DOTween.PlayBackwards(this.gameObject);
    }
}
