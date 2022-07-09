using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandManager : MonoBehaviour
{    
    public GameObject spriteCardPrefab;
    //  public List<GameObject> listOfCardsOnHand;
    public Dictionary<string, GameObject> listOfCardsOnHand = new Dictionary<string, GameObject>();


    public GameObject explosionEffectPrefab;

    private string currentCardID;
    private GameObject currentCard;

    private Deck handDeck;
    private float maxDepth;

    CardPiles cardPilesData;



    void Start()
    {
        Debug.Log("[HandManager]Start");
        //GameManager.Instance.EVENT_CARD_MOUSE_ENTER.AddListener(OnCardMouseEnter);
       // GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
        GameManager.Instance.EVENT_CARD_DESTROYED.AddListener(OnCardDestroyed);
       
      
    }

    private void OnCardDestroyed(string cardId)
    {
        Debug.Log("[Removing card "+cardId+" from hand]");
        //listOfCardsOnHand.Remove(listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId)));
        listOfCardsOnHand.Remove(cardId);

      
        
    }

    private void RelocateCards()
    {
        
        float counter = 0;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float halfWidth = listOfCardsOnHand.Values.Count * GameSettings.HAND_CARD_GAP/2;
        float offset = listOfCardsOnHand.Values.Count % 2 == 0 ? GameSettings.HAND_CARD_GAP / 2 : 0;

        Debug.Log("----------------------------Relocate cards halfWidth=" + halfWidth);

        foreach (GameObject card in listOfCardsOnHand.Values)        
        {
            Vector3 pos = Vector3.zero;
            pos.x = counter * GameSettings.HAND_CARD_GAP - halfWidth + offset;
            pos.y = Camera.main.orthographicSize * -1;
            pos.z = depth;
            card.transform.position = pos;

            //var angle = (float)(counter * Mathf.PI * 2);                   
            var angle = (float)(pos.x * Mathf.PI * 2);                   


            //newCard.transform.position = pos;
            Vector3 rot = card.transform.eulerAngles;
            rot.z = angle / -2;
            card.transform.eulerAngles = rot;
            card.GetComponent<CardOnHandManager>().targetRotation = rot;
            card.GetComponent<CardOnHandManager>().targetPosition = pos;
            card.transform.localScale = Vector3.one;

            counter++;
            depth--;

        }
    }

    private void Awake()
    {
        Debug.Log("[HandManager]Awake");
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
    }

    private void OnEnable()
    {
        Debug.Log("[HandManager]OnEnable");
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private void OnDrawCards()
    {
        if (cardPilesData == null) return;
        Debug.Log("**********************************************[OnDrawCards] " );
        //Generate cards hand
        listOfCardsOnHand.Clear();
        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        Vector3 spawnPosition = GameSettings.HAND_CARDS_GENERATION_POINT;

        float counter = handDeck.cards.Count / -2;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        foreach (Card card in handDeck.cards)
        //for (var i= 0; i < (handDeck.cards.Count - 4);i++)
        {
           // var card = handDeck.cards[3];
            var angle = (float)(counter * Mathf.PI * 2);
            //Debug.Log(counter + "/" + angle);
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(card.cardId,newCard);
            newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
            Vector3 pos = newCard.transform.position;
            pos.x = counter * 2.2f;//TODO: this value has to be beased on the number of cards to display them from the center
                                   // pos.y = (Mathf.Cos(angle * Mathf.Deg2Rad) * handDeck.cards.Count) - 9.5F;//TODO:magic numbers 5,9
            pos.y = Camera.main.orthographicSize * -1;//TODO:magic numbers 5,9
            pos.z = depth;
            newCard.GetComponent<CardOnHandManager>().targetPosition = pos;//we need this to put the card back after mouse interaction
            newCard.transform.position = spawnPosition;

            Debug.Log("[***************************************************************Moving cards from Draw creation]");
            newCard.GetComponent<CardOnHandManager>().MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand,true, pos, delay);

            Vector3 rot = newCard.transform.eulerAngles;
            rot.z = angle / -2;
            newCard.transform.eulerAngles = rot;
            newCard.GetComponent<CardOnHandManager>().targetRotation = rot;

            delay -= delayStep;

            counter++;
            depth--;

        }

        maxDepth = --depth;
    }

    private void OnCardsPilesUpdated(CardPiles data)
    {
        Debug.Log("**********************************************[OnCardsPilesUpdated] ");
        cardPilesData = data;

        if (cardPilesData.data.hand.Count > listOfCardsOnHand.Count)
        {
           /* foreach (GameObject obj in listOfCardsOnHand.Values)
            {
                Destroy(obj);
            }*/

            listOfCardsOnHand.Clear();
            OnDrawCards();
        }

    }

   /* private void OnDrawCardsCalled()
    {
        GameManager.Instance.
    }*/

  /*  private void OnCardMouseExit(string cardId)
    {
        //Debug.Log("[-----OnCardMouseExit]cardid=" + cardId);

        foreach (GameObject go in listOfCardsOnHand.Values)
        {
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();

           go.transform.DOMove(cardData.targetPosition, 0.1f);
           go.transform.DORotate(cardData.targetRotation, 0.2f);
           go.transform.DOScale(Vector3.one , 0.2f);

        }
    }*/

   /* private void OnCardMouseEnter(string cardId)
    {
        // Debug.Log("[++++++OnCardMouseEnter]cardid="+cardId);
        // GameObject selectedCard = listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId));
        GameObject selectedCard = listOfCardsOnHand[cardId];
       

        foreach (GameObject go in listOfCardsOnHand.Values)
        {            
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();
            go.transform.DOMove(cardData.targetPosition, 0.1f);
            //go.transform.position = cardData.targetPosition;

            if (cardData.th != cardId)
            {
                float xx = go.transform.position.x - selectedCard.transform.position.x;
               // Debug.Log("---this card is the " + (xx > 0 ? "left" : "right"));
                float movex = xx > 0 ? 0.5f : -0.5f;
                Vector3 pos = cardData.targetPosition;
                pos.x += movex;
                
                go.transform.DOMove(pos, 0.1f);
            }
            else
            {
                go.transform.DOScale(Vector3.one*1.25f,0.2f);//TODO:magic number for scale,move to settings
                Vector3 pos = cardData.targetPosition;
                pos.y += 1.5f;
                pos.z = maxDepth;
                go.transform.DOMove(pos,0.2f);

                go.transform.DORotate(Vector3.zero,0.2f);
            }
        }
    }*/
   
}
