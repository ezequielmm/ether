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

    public Deck handDeck;
    public Deck drawDeck;
    public Deck discardDeck;
    public Deck exhaustDeck;

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
        Debug.Log("[Removing card "+cardId+" from hand]");
        //listOfCardsOnHand.Remove(listOfCardsOnHand.Find((x) => (x.GetComponent<CardOnHandManager>().id == cardId)));
        Destroy(listOfCardsOnHand[cardId]);
        listOfCardsOnHand.Remove(cardId);
        RelocateCards();      
        
    }

    private void Awake()
    {
        Debug.Log("[HandManager]Awake");
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
        GameManager.Instance.EVENT_CARD_DRAW.AddListener(OnCardDraw);
        GameManager.Instance.EVENT_CARD_CREATE.AddListener(CreateCard);
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

    private void CreateCard(string cardID)
    {
        Debug.Log("Create card "+cardID);
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
        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        drawDeck = new Deck();
        drawDeck.cards = cardPilesData.data.draw;

        discardDeck = new Deck();
        discardDeck.cards = cardPilesData.data.discard;

        exhaustDeck = new Deck();
        exhaustDeck.cards = cardPilesData.data.exhaust;

        Vector3 spawnPosition = GameSettings.HAND_CARDS_GENERATION_POINT;

        float counter = handDeck.cards.Count / -2;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        Debug.Log("[OnDrawCards] listOfCardsOnHand.Count:" + listOfCardsOnHand.Count);

        foreach (Card card in handDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                Debug.Log("[HandManager | Hand Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
            }
                     
        }
        foreach (Card card in drawDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                Debug.Log("[HandManager | Draw Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false);//disable and not notify
            }    
        }
        foreach (Card card in discardDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                Debug.Log("[HandManager | Discard Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false);//disable and not notify
            }
        }
        foreach (Card card in exhaustDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                Debug.Log("[HandManager | Exhaust Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false);//disable and not notify
            }
        }

        RelocateCards(true);
    }

    /// <summary>
    /// Relocates the cards in hands. If move is on, card movement is send to the cards themselves to be preformed.
    /// </summary>
    /// <param name="move">True to do a draw animation to hand.</param>
    private void RelocateCards(bool move = false)
    {

        float counter = 0;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float halfWidth = handDeck.cards.Count * GameSettings.HAND_CARD_GAP / 2;

        string result = handDeck.cards.Count % 2 == 0 ? "even" : "odd";

        //Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++handDeck.cards.Count=" + handDeck.cards.Count+" is "+ result);

        //float offset = handDeck.cards.Count % 2 == 0 ?  GameSettings.HAND_CARD_GAP / 2 : GameSettings.HAND_CARD_GAP/2;
        float offset = GameSettings.HAND_CARD_GAP/2;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        // Debug.Log("----------------------------Relocate cards offset=" + offset);
        foreach (Card cardData in handDeck.cards)
        {
            //  foreach (GameObject card in listOfCardsOnHand.Values)
            //  {
            GameObject card;
            if (listOfCardsOnHand.TryGetValue(cardData.id, out card))
            {
                Vector3 pos = Vector3.zero;
                pos.x = counter * GameSettings.HAND_CARD_GAP - halfWidth + offset;
                pos.y = GameSettings.HAND_CARD_REST_Y;
               // pos.y = Camera.main.orthographicSize * Mathf.Cos(pos.x);
                pos.z = depth;
                //card.transform.position = pos;

                //var angle = (float)(counter * Mathf.PI * 2);                   
                var angle = (float)(pos.x * Mathf.PI * 2);
                pos.y += Mathf.Cos(pos.x * GameSettings.HAND_CARD_Y_CURVE);


                //newCard.transform.position = pos;
                Vector3 rot = card.transform.eulerAngles;
                rot.z = angle / -2;
                card.transform.eulerAngles = rot;
                card.GetComponent<CardOnHandManager>().targetRotation = rot;
                card.GetComponent<CardOnHandManager>().targetPosition = pos;
                card.transform.localScale = Vector3.one;

                counter++;
                depth -= GameSettings.HAND_CARD_SPRITE_Z_INTERVAL;

                if (move)
                {
                    card.GetComponent<CardOnHandManager>().MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, true, pos, delay);
                    delay -= delayStep;
                }
                else
                {
                    card.transform.DOMove(pos,0.3f);
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
                

    }   
}
