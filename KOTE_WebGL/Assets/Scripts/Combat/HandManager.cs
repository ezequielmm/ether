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

    

    void Start()
    {
      
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeUpdate);//this is called after a node is slected on the map and get an asnwer from server
        GameManager.Instance.EVENT_CARD_MOUSE_ENTER.AddListener(OnCardMouseEnter);
        GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
    }

    private void OnCardMouseExit(string cardId)
    {
        Debug.Log("[-----OnCardMouseExit]cardid=" + cardId);

        foreach (GameObject go in listOfCardsOnHand)
        {
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();

            cardData.mySequence.Append(go.transform.DOMove(cardData.targetPosition, 0.1f));
            cardData.mySequence.Append(go.transform.DORotate(cardData.targetRotation, 0.2f));
            cardData.mySequence.Append(go.transform.DOScale(Vector3.one , 0.2f));

        }
    }

    private void OnCardMouseEnter(string cardId)
    {
        Debug.Log("[++++++OnCardMouseEnter]cardid="+cardId);
        GameObject selectedCard = listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId));

        if (selectedCard == null) return;

        foreach (GameObject go in listOfCardsOnHand)
        {            
            CardOnHandManager cardData = go.GetComponent<CardOnHandManager>();
            cardData.mySequence.Append(go.transform.DOMove(cardData.targetPosition, 0.1f));
            //go.transform.position = cardData.targetPosition;

            if (cardData.id != cardId)
            {
                float xx = go.transform.position.x - selectedCard.transform.position.x;
               // Debug.Log("---this card is the " + (xx > 0 ? "left" : "right"));
                float movex = xx > 0 ? 0.5f : -0.5f;
                Vector3 pos = cardData.targetPosition;
                pos.x += movex;
                
                cardData.mySequence.Append(go.transform.DOMove(pos, 0.1f));
            }
            else
            {
                cardData.mySequence.Append(go.transform.DOScale(Vector3.one*1.2f,0.2f));//TODO:magic number for scale,move to settings
                Vector3 pos = cardData.targetPosition;
                pos.y += 1.5f;
                cardData.mySequence.Append(go.transform.DOMove(pos,0.2f));

                cardData.mySequence.Append(go.transform.DORotate(Vector3.zero,0.2f));
            }
        }
    }

    private void OnParticleSystemStopped()
    {
        Debug.Log("lalal");
    }

    private void OnNodeUpdate(NodeStateData nodeState, bool initialCall)
    {
        float effectDelay = 0.5f;
        foreach (GameObject go in listOfCardsOnHand)
        {
            if (go == null) continue;
            GameObject fireball = Instantiate(explosionEffectPrefab,go.transform);
            fireball.transform.position = go.transform.position;
            fireball.transform.localScale = fireball.transform.localScale*0.5f;
   
            effectDelay += 0.3f;
        }

        listOfCardsOnHand = new List<GameObject>();
        Deck handDeck = new Deck();
        handDeck.cards = nodeState.data.data.player.cards.hand;

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
            newCard.GetComponent<CardOnHandManager>().populate(card,nodeState.data.data.player.energy);
            Vector3 pos = newCard.transform.position;
            pos.x = counter * 2.2f;//TODO:magic numbers and make it related to the number of cards to center
            pos.y = (Mathf.Cos(angle * Mathf.Deg2Rad) * 5) - 9;//TODO:magic numbers
            pos.z = depth;

            newCard.GetComponent<CardOnHandManager>().targetPosition = pos;

            if (initialCall)
            {
                newCard.transform.position = spawnPosition;

                newCard.transform.DOMove(pos, .5f).SetDelay(delay, true).SetEase(Ease.OutBack).OnComplete(newCard.GetComponent<CardOnHandManager>().ActivateCard);
                newCard.transform.DOPlay();
            }
            else
            {
                newCard.transform.position = pos;
                newCard.GetComponent<CardOnHandManager>().ActivateCard();
            }      

            //newCard.transform.position = pos;
            Vector3 rot = newCard.transform.eulerAngles;
            rot.z = angle / -2;
            newCard.transform.eulerAngles = rot;
            newCard.GetComponent<CardOnHandManager>().targetRotation = rot;

            delay -= delayStep;

            counter++;
            depth--;

        }
    }
   
}
