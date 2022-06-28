using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    
    public GameObject spriteCardPrefab;
    public List<GameObject> listOfCardsOnHand;
    public GameObject explosionEffectPrefab;

    private string currentCardID;
    private GameObject currentCard;

    private Deck handDeck;
    private float maxDepth;
        

    void Start()
    {
        Debug.Log("[HandManager]Start");
        GameManager.Instance.EVENT_CARD_MOUSE_ENTER.AddListener(OnCardMouseEnter);
        GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);      
      
    }

    private void Awake()
    {
        Debug.Log("[HandManager]Awake");
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
    }

    private void OnEnable()
    {
        Debug.Log("[HandManager]OnEnable");
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private void OnCardsPilesUpdated(CardPiles data)
    {
        Debug.Log("[OnCardsPilesUpdated] "+data);
        //Generate cards hand
        listOfCardsOnHand = new List<GameObject>();
        handDeck = new Deck();
        handDeck.cards = data.data.hand;

        Vector3 spawnPosition = GameSettings.HAND_CARDS_GENERATION_POINT;

        float counter = handDeck.cards.Count / -2;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        foreach (Card card in handDeck.cards)
        {
            var angle = (float)(counter * Mathf.PI * 2);
            //Debug.Log(counter + "/" + angle);
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(newCard);
            newCard.GetComponent<CardOnHandManager>().populate(card, data.data.energy);
            Vector3 pos = newCard.transform.position;
            pos.x = counter * 2.2f;//TODO: this value has to be beased on the number of cards to display them from the center
           // pos.y = (Mathf.Cos(angle * Mathf.Deg2Rad) * handDeck.cards.Count) - 9.5F;//TODO:magic numbers 5,9
            pos.y = Camera.main.orthographicSize*-1;//TODO:magic numbers 5,9
            pos.z = depth;

            newCard.GetComponent<CardOnHandManager>().targetPosition = pos;

            newCard.transform.position = spawnPosition;

            newCard.transform.DOMove(pos, .5f).SetDelay(delay, true).SetEase(Ease.OutBack).OnComplete(newCard.GetComponent<CardOnHandManager>().ActivateCard);
            newCard.transform.DOPlay(); 

            //newCard.transform.position = pos;
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

   /* private void OnDrawCardsCalled()
    {
        GameManager.Instance.
    }*/

    private void OnCardMouseExit(string cardId)
    {
        //Debug.Log("[-----OnCardMouseExit]cardid=" + cardId);

        foreach (GameObject go in listOfCardsOnHand)
        {
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();

           go.transform.DOMove(cardData.targetPosition, 0.1f);
           go.transform.DORotate(cardData.targetRotation, 0.2f);
           go.transform.DOScale(Vector3.one , 0.2f);

        }
    }

    private void OnCardMouseEnter(string cardId)
    {
       // Debug.Log("[++++++OnCardMouseEnter]cardid="+cardId);
        GameObject selectedCard = listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId));

        if (selectedCard == null) return;

        foreach (GameObject go in listOfCardsOnHand)
        {            
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();
           go.transform.DOMove(cardData.targetPosition, 0.1f);
            //go.transform.position = cardData.targetPosition;

            if (cardData.id != cardId)
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
    }

    private void OnParticleSystemStopped()
    {
        Debug.Log("lalal");
    }
      
    private void OnNodeUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        //do we had cards on the current hand?
        if (listOfCardsOnHand.Count > 0)//this is insecure. The list could have null elements and the count still would be above 0 
        {
            //compare new hand cards with current hand cards
            Deck newHandDeck = new Deck();
            newHandDeck.cards = nodeState.data.data.player.cards.hand;
            int counterOfDifferentCards = 0;
            foreach (Card newCard in newHandDeck.cards)
            {
                foreach (GameObject go in listOfCardsOnHand)
                {

                }
            }
        }
        else
        {
            //
        }      


            //play effect on current cards. This must be isolated to only happen when it is the end of turn but not on card played
        float effectDelay = 0.5f;
        foreach (GameObject go in listOfCardsOnHand)
        {
            if (go == null) continue;//avoid working on a gameobject that has been destroyed but is still referenced on the list
            GameObject fireball = Instantiate(explosionEffectPrefab,go.transform);
            fireball.transform.position = go.transform.position;
            fireball.transform.localScale = fireball.transform.localScale*0.5f;
   
            effectDelay += 0.3f;
        }

        

    }
   
}
