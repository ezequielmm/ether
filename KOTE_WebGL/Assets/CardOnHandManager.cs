using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardOnHandManager : MonoBehaviour
{
    public TextMeshPro energyTF;
    public TextMeshPro nameTF;
    public TextMeshPro rarityTF;
    public TextMeshPro descriptionTF;
    public string id;

    public Vector3 targetPosition;
    public bool cardActive = false;
       

    // Start is called before the first frame update
    void Start()
    {
        
    }

    internal void populate(Card card)
    {
        energyTF.SetText(card.energy.ToString());
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
        this.id = card.id;
    }

    private void OnMouseEnter()
    {
        if (cardActive)
        {
            DOTween.PlayForward(this.gameObject);
        }
           
    }
    private void OnMouseExit()
    {
        if (cardActive)
        { 
            DOTween.PlayBackwards(this.gameObject); 
        }
           
    }

    private void OnMouseDown()
    {
        if (cardActive) {
            GameManager.Instance.EVENT_CARD_PLAYED.Invoke(id);
        }
        
    }

    public void ActivateCard()
    {
        Debug.Log("Activating card");
        cardActive = true;
    }
}
