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
   // public Dictionary<string, GameObject> listOfCardsOnDraw = new Dictionary<string, GameObject>();


    public GameObject explosionEffectPrefab;

    private string currentCardID;
    private GameObject currentCard;

    public  Deck handDeck;
    public  Deck drawDeck;
  
    CardPiles cardPilesData;

    int cardsDrawn = 0;
    bool audioRunning = false;

    void Start()
    {
        Debug.Log("[HandManager]Start");
        //GameManager.Instance.EVENT_CARD_MOUSE_ENTER.AddListener(OnCardMouseEnter);
       // GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
        GameManager.Instance.EVENT_CARD_DISABLED.AddListener(OnCardDestroyed);
       
      
    }

    private void OnCardDestroyed(string cardId)
    {
        //Debug.Log("[Removing card "+cardId+" from hand]");
        //listOfCardsOnHand.Remove(listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId)));
        listOfCardsOnHand.Remove(cardId);
        RelocateCards();      
        
    }

    private void Awake()
    {
        Debug.Log("[HandManager]Awake");
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
        GameManager.Instance.EVENT_CARD_DRAW.AddListener(OnCardDraw);
    }

    private void OnEnable()
    {
        Debug.Log("[HandManager]OnEnable");
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private void OnCardDraw()
    {
        Debug.Log($"[Hand Pile] Card Drawn.");
        cardsDrawn++;
        StartCoroutine(DrawCardSFX());
    }

    private IEnumerator DrawCardSFX() 
    {
        if (!audioRunning)
        {
            audioRunning = true;
            for (; cardsDrawn >= 0; cardsDrawn--)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Draw");
                yield return new WaitForSeconds(GameSettings.CARD_SFX_MIN_RATE);
            }
            if (cardsDrawn < 0) 
            {
                cardsDrawn = 0;
            }
            audioRunning = false;
        }
    }

    private void OnDrawCards()
    {
        if (cardPilesData == null)
        {
            Debug.Log("[OnDrawCards]No cards data at all. Retrieving");
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
            Invoke("OnDrawCards",0.2f);
            return;
        }
        else if(cardPilesData.data.hand.Count < 1)
        {
            Debug.Log("[OnDrawCards]No hands cards data. Retrieving");
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
            Invoke("OnDrawCards", 0.2f);
            return;
        }
        Debug.Log("**********************************************[OnDrawCards]draw.count: "+ cardPilesData.data.draw.Count+", hand.count:"+cardPilesData.data.hand.Count);
        //Generate cards hand
        listOfCardsOnHand.Clear();
        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        drawDeck = new Deck();
        drawDeck.cards = cardPilesData.data.draw;

        Vector3 spawnPosition = GameSettings.HAND_CARDS_GENERATION_POINT;

        float counter = handDeck.cards.Count / -2;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;


        /*foreach (Card card in handDeck.cards)
        //for (var i= 0; i < (handDeck.cards.Count - 4);i++)
        {
           // var card = handDeck.cards[3];
            var angle = (float)(counter * Mathf.PI * 2);
            Debug.Log("card.cardId: "+card.id);

            
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(card.id,newCard);


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

        }*/
        foreach (Card card in handDeck.cards)
        {
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(card.id, newCard);
            newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
        }

        foreach (Card card in drawDeck.cards)
        {
            GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
            listOfCardsOnHand.Add(card.id, newCard);
            newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
            newCard.GetComponent<CardOnHandManager>().DisableCardContent(false);//disable and not notify
        }

        RelocateCards(true);
    }

    private void RelocateCards(bool move = false)
    {

        float counter = 0;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float halfWidth = handDeck.cards.Count * GameSettings.HAND_CARD_GAP / 2;
        float offset = handDeck.cards.Count % 2 == 0 ? 0 : GameSettings.HAND_CARD_GAP / 2 ;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

         Debug.Log("----------------------------Relocate cards offset=" + offset);
        foreach (Card cardData in handDeck.cards)
        {
            //  foreach (GameObject card in listOfCardsOnHand.Values)
            //  {
            GameObject card;
            if (listOfCardsOnHand.TryGetValue(cardData.id, out card))
            {
                Vector3 pos = Vector3.zero;
                pos.x = counter * GameSettings.HAND_CARD_GAP - halfWidth + offset;
                pos.y = Camera.main.orthographicSize * -1;
               // pos.y = Camera.main.orthographicSize * Mathf.Cos(pos.x);
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

                if (move)
                {
                    card.GetComponent<CardOnHandManager>().MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, true, pos, delay);
                    delay -= delayStep;
                }
            }    

        }
    }

    private void OnCardsPilesUpdated(CardPiles data)
    {
      // Debug.Log("**********************************************[OnCardsPilesUpdated] ");
        cardPilesData = data;

        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        drawDeck = new Deck();
        drawDeck.cards = cardPilesData.data.draw;

      /*  if (cardPilesData.data.hand.Count > listOfCardsOnHand.Count)
        {
            listOfCardsOnHand.Clear();
            OnDrawCards();
        }*/

    }   
}
