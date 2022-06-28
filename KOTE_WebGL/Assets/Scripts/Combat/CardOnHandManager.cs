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
    public string type;
    public int card_energy_cost;
    public bool card_can_be_played = true;

    public Vector3 targetPosition;
    public Vector3 targetRotation;
    public bool cardActive = false;

    [Header("Outline effects")]
    public ParticleSystem auraPS;
    public GameObject cardBgGO;
    public Material greenOutlineMaterial;
    public Material blueOutlineMaterial;
   
    private Material defaultMaterial;
    private Material outlineMaterial;      

    [Header ("Colors")]
    public Color greenColor;
    public Color blueColor;
    public Color redColor;

    [HideInInspector]
    public Sequence mySequence;

    // Start is called before the first frame update
    void Start()
    {
        mySequence = DOTween.Sequence();

        defaultMaterial = cardBgGO.GetComponent<Renderer>().sharedMaterial;
    }

    internal void populate(Card card, int energy)
    {
        energyTF.SetText(card.energy.ToString());
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
        this.id = card.id;
        type = card.card_type;

        //Energy management
      //  Debug.Log("card energy="+card.energy+", energy="+energy);
        if (card.energy <= energy)
        {
            var main = auraPS.main;
            main.startColor = greenColor;
            outlineMaterial = greenOutlineMaterial;//TODO:apply blue if card has a special condition
            auraPS.gameObject.SetActive(true);
            card_can_be_played = true;
        }
        else
        {
            auraPS.gameObject.SetActive(false);
            energyTF.color = redColor;
            outlineMaterial = greenOutlineMaterial;
            card_can_be_played = false;
        }

        card_energy_cost = card.energy;
    }

    private void OnMouseEnter()
    {
        if (cardActive)
        {
           // DOTween.PlayForward(this.gameObject);
            GameManager.Instance.EVENT_CARD_MOUSE_ENTER.Invoke(this.id);
            if(auraPS.gameObject.activeSelf)auraPS.Play();

            cardBgGO.GetComponent<Renderer>().material = outlineMaterial;
        }
           
    }
    private void OnMouseExit()
    {
        if (cardActive)
        { 
           // DOTween.PlayBackwards(this.gameObject);
            GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(this.id);
            if (auraPS.gameObject.activeSelf) auraPS.Stop();
            cardBgGO.GetComponent<Renderer>().material = defaultMaterial;
        }
           
    }

    private void OnMouseDrag()
    {

        if (!card_can_be_played)
        {
            //TODO: show no energy message
            return;
        }

        float xxDelta = Mathf.Abs(this.transform.position.x - targetPosition.x);


       // Debug.Log("Distance y is " + xxDelta);
       if (type == "attack")
       {
            
           //show the pointer instead of following the mouse
           GameManager.Instance.EVENT_CARD_ACTIVATE_POINTER.Invoke(transform.position);
           return;
       }

        float zz = this.transform.position.z;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = zz;
        this.transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        if (type == "attack")
        {
            GameManager.Instance.EVENT_CARD_DEACTIVATE_POINTER.Invoke(id);
        }
    }

    private void OnMouseUpAsButton()
    {
        if (cardActive)
        {
            if (Vector2.Distance(this.transform.position,Vector2.zero) < 1.5f)
            {
                Debug.Log("card is on center");
                GameManager.Instance.EVENT_CARD_PLAYED.Invoke(id,-1);
              //  Destroy(this.gameObject);//TODO don destroy unless message back is error free
            }
            else
            {
                Debug.Log("card is far from center");
                //MoveCardBackToOriginalHandPosition();
            }
            
        }
    }
    private void OnMouseDown()
    {
       
       
    }

    private void MoveCardBackToOriginalHandPosition()
    {
       //this.transform.DOMove(originalPosition, 0.5f);
    }

    public void ActivateCard()
    {
       // Debug.Log("Activating card");
        cardActive = true;
    }
}
