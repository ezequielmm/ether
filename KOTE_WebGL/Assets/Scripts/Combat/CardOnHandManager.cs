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

    private Vector3 drawPileOrthoPosition;
    private Vector3 discardPileOrthoPosition;
    private Vector3 exhaustPileOrthoPosition;

    [Header("Movement")]
    public ParticleSystem movePs;

    private void Awake()
    {

        //Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
        //Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.

        drawPileOrthoPosition = TransformUIToOrtho("DrawCardPile");
        discardPileOrthoPosition = TransformUIToOrtho("DiscardCardPile");
        exhaustPileOrthoPosition = TransformUIToOrtho("ExhaustedPilePrefab");

    }



    // Start is called before the first frame update
    void Start()
    {            

        mySequence = DOTween.Sequence();

        defaultMaterial = cardBgGO.GetComponent<Renderer>().sharedMaterial;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnUpdateEnergy);
        GameManager.Instance.EVENT_MOVE_CARD.AddListener(OnCardToMove);

    }

    private void OnCardToMove(CardToMoveData data)
    {
        if (this.id == data.card_id)
        {
            System.Enum.TryParse(data.source, out CARDS_POSITIONS_TYPES origin);
            System.Enum.TryParse(data.destination, out CARDS_POSITIONS_TYPES destination);
          
            MoveCard(origin, destination);
        }

       
    }

    private void OnUpdateEnergy(int currentEnergy, int maxEnergy)
    {
        Debug.Log("[CardOnHandManager] OnUpdateEnergy = "+currentEnergy);
       
        UpdateCardBasedOnEnergy(currentEnergy);
    }

    internal void Populate(Card card, int energy)
    {
        energyTF.SetText(card.energy.ToString());
        nameTF.SetText(card.name);
        rarityTF.SetText(card.rarity);
        descriptionTF.SetText(card.description);
        this.id = card.id;
        type = card.card_type;
        card_energy_cost = card.energy;

        UpdateCardBasedOnEnergy( energy);      
        
    }

    public void MoveCard(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType, Vector3 pos = default(Vector3), float delay = 0)
    {
        Debug.Log("[CardOnHandManager] MoveCard = " + originType+" to "+destinationType);
        movePs.Play();

        Vector3 origin = new Vector3();
        Vector3 destination = new Vector3();

        switch (originType)
        {
            case CARDS_POSITIONS_TYPES.draw:
                origin = drawPileOrthoPosition;
                break;
            case CARDS_POSITIONS_TYPES.discard:
                origin = discardPileOrthoPosition;
                break;
            case CARDS_POSITIONS_TYPES.hand:
                origin = this.transform.position;
                break;
            case CARDS_POSITIONS_TYPES.exhaust:
                origin = exhaustPileOrthoPosition;
                break;
        }

        if (pos.magnitude > 0)
        {
            destination = pos;
        }
        else
        {

            switch (destinationType)
            {
                case CARDS_POSITIONS_TYPES.draw:
                    destination = drawPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.discard:
                    destination = discardPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    destination = pos;
                    break;
                case CARDS_POSITIONS_TYPES.exhaust:
                    destination = exhaustPileOrthoPosition;
                    break;
            }
        }


        this.transform.position = origin;      
        
        //Debug.Log("Moving card")
        

        if (delay > 0)
        {
            transform.DOMove(destination, 1f).SetDelay(delay, true).SetEase(Ease.InCirc).OnComplete(DeactivatePS); ;
           // transform.DOMoveX(destination.x, .5f).SetEase(Ease.Linear);
           // transform.DOMoveY(destination.y, .5f).SetEase(Ease.InCirc);
        }
        else
        {

            transform.DOMove(destination, 1f).SetEase(Ease.OutCirc);
            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic).OnComplete(HideAndDeactivateCard);
        }

        //transform.DOPlay();
    }

    private void DeactivatePS()
    {
        movePs.Stop();
    }

    private void HideAndDeactivateCard()
    {
        //cardActive = false;
        //this.gameObject.SetActive(false);
        Destroy(this.gameObject);

    }
    private void UpdateCardBasedOnEnergy( int energy)
    {
        if (card_energy_cost <= energy)
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

    private Vector3 TransformUIToOrtho(string uiName)
    {
        Vector3 pos = GameObject.Find(uiName).transform.position;//(1.1, 104.5, 0.0)

        float height = 2 * Camera.main.orthographicSize;//10
        float width = height * Camera.main.aspect;//21.42


        //transform UI coordinates to orthorgraphic coordinates
        float xx = pos.x * width / Screen.width;
        xx -= width / 2;//ortho counts from the center 
        float yy = pos.y * height / Screen.height;
        yy -= height / 2;

        return new Vector3(xx, yy, this.transform.position.x);
    }
}
