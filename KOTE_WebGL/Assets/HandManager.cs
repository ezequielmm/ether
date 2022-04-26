using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    
    public GameObject spriteCardPrefab;
    public List<GameObject> listOfCardsOnHand;
    void Start()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeUpdate);//this is called after a node is slected on the map and get an asnwer from server
    }

    private void OnNodeUpdate(NodeStateData nodeState)
    {
        listOfCardsOnHand = new List<GameObject>();
        Deck handDeck = new Deck();
        handDeck.cards= nodeState.data.data.player.cards.hand;

        Vector3 spawnPosition = new Vector3(-7,-5,-9);
           
               
        float counter = handDeck.cards.Count/-2;
        float depth = -12;
        float delayStep = 0.1f;
        float delay = delayStep* handDeck.cards.Count;

        foreach (Card card in handDeck.cards)
        {
            var angle = (float)(counter * Mathf.PI * 2);
            Debug.Log(counter + "/" + angle);
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(newCard);
            newCard.GetComponent<CardOnHandManager>().populate(card);
            Vector3 pos = newCard.transform.position;
            pos.x = counter*2.2f;
            pos.y = (Mathf.Cos(angle*Mathf.Deg2Rad)*5)-9;
            pos.z = depth;

            newCard.GetComponent<CardOnHandManager>().targetPosition = pos;

            newCard.transform.position = spawnPosition;

            newCard.transform.DOMove(pos, .5f).SetDelay(delay,true).SetEase(Ease.OutBack).OnComplete(newCard.GetComponent<CardOnHandManager>().ActivateCard);
            newCard.transform.DOPlay();

            //newCard.transform.position = pos;
            Vector3 rot = newCard.transform.eulerAngles;
            rot.z = angle/-2;
            newCard.transform.eulerAngles = rot;

            delay -= delayStep;

            counter++;
            depth--;

        }       

        Debug.Log(this.transform);

    }
   
}
