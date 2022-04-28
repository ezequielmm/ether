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

    private Vector3 originalPosition;


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

    private void OnMouseDrag()
    {
        float xxDelta = Mathf.Abs(this.transform.position.x - originalPosition.x);

        if (xxDelta > GameSettings.HAND_CARD_MAX_XX_DRAG_DELTA)
        {
            MoveCardBackToOriginalHandPosition();
            return;
        }

       // Debug.Log("Distance y is " + xxDelta);

        float zz = this.transform.position.z;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = zz;
        this.transform.position = mousePos;
    }

    private void OnMouseUpAsButton()
    {
        if (cardActive)
        {
            if (Vector2.Distance(this.transform.position,Vector2.zero) < 1.5f)
            {
                Debug.Log("card is on center");
                GameManager.Instance.EVENT_CARD_PLAYED.Invoke(id);
                Destroy(this.gameObject);//TODO don destroy unless message back is error free
            }
            else
            {
                Debug.Log("card is far from center");
                MoveCardBackToOriginalHandPosition();
            }
           // 
        }
    }
    private void OnMouseDown()
    {
        originalPosition = this.transform.position;
       
    }

    private void MoveCardBackToOriginalHandPosition()
    {
       this.transform.DOMove(originalPosition, 0.5f);
    }

    public void ActivateCard()
    {
        Debug.Log("Activating card");
        cardActive = true;
    }
}
