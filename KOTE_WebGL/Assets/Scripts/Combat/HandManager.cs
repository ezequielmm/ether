using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject spriteCardPrefab;

    // Hand Manager and CardMovementManager are one module, but are split for cleanlieness
    public CardMovementManager cardMovementManager;

    //  public List<GameObject> listOfCardsOnHand;
    public Dictionary<string, GameObject>
        listOfCardsOnHand = new Dictionary<string, GameObject>(); // CardID <--> Card Obj
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

    private bool requestTimerIsRunning;
    private bool requestAgain;
    private Coroutine requestTimer;

    private void OnCardDestroyed(string cardId)
    {
        Debug.Log("[HandManager] Removing card " + cardId + " from hand");

        var cardMoved = handDeck.cards.Find(card => card.id == cardId);
        if (cardMoved != null)
        {
            handDeck.cards.Remove(cardMoved);
            discardDeck.cards.Add(cardMoved);
        }

        StartCoroutine(RelocateCards());
    }

    private void Awake()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
        GameManager.Instance.EVENT_CARD_DRAW.AddListener(OnCardDraw); // SFX
        GameManager.Instance.EVENT_CARD_ADD.AddListener(OnCardAdd);
        GameManager.Instance.EVENT_CARD_DISABLED.AddListener(OnCardDestroyed);
        // if we're adding a card to the hand that isn't a draw
        GameManager.Instance.EVENT_REARRANGE_HAND.AddListener(OnRearrangeHand);
    }

    private void Start()
    {
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private void OnCardDraw()
    {
        //Debug.Log($"[Hand Pile] Card Drawn.");
        cardsDrawn++;
        StartCoroutine(DrawCardSFX());
    }

    private IEnumerator DrawCardSFX()
    {
        if (!audioRunning)
        {
            audioRunning = true;

            if (cardsDrawn == 1)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Draw Single");
            }
            else if (cardsDrawn > 1)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Draw Multiple");
            }

            yield return new WaitForSeconds(GameSettings.CARD_SFX_MIN_RATE);

            cardsDrawn = 0;
            audioRunning = false;
        }
    }

    private void OnCardAdd(AddCardData addCardData)
    {
        // create the new card in the middle of the screen
        GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
        listOfCardsOnHand.Add(addCardData.card.id, newCard);
        CardOnHandManager cardManager = newCard.GetComponent<CardOnHandManager>();
        cardManager.Populate(addCardData.card, cardPilesData.data.energy);
        cardManager.ShowAddedCard(addCardData);
    }

    private void OnRearrangeHand()
    {
        StartCoroutine(RelocateCards());
    }

    private void OnDrawCards()
    {
        Debug.Log("[HandManager]------------------------------------------------>[OnDrawCards]");
        if (cardPilesData == null)
        {
            Debug.Log("[HandManager] No cards data at all. Requesting Card Piles");
            RequestCardsPilesData();
            return;
        }
        else if (cardPilesData.data.hand.Count < 1)
        {
            Debug.Log("[HandManager] No hands cards data. Requesting Card Piles");
            RequestCardsPilesData();
            return;
        }

        Debug.Log(
            $"[HandManager] Card Piles Retrieved. draw.count: {cardPilesData.data.draw.Count} | hand.count: {cardPilesData.data.hand.Count} | discard.count: {cardPilesData.data.discard.Count} | exhaust.count: {cardPilesData.data.exhausted.Count}");
        requestAgain = false;

        //Generate cards hand
        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        drawDeck = new Deck();
        drawDeck.cards = cardPilesData.data.draw;

        discardDeck = new Deck();
        discardDeck.cards = cardPilesData.data.discard;

        exhaustDeck = new Deck();
        exhaustDeck.cards = cardPilesData.data.exhausted;

        Vector3 spawnPosition = GameSettings.HAND_CARDS_GENERATION_POINT;

        float counter = handDeck.cards.Count / -2;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        foreach (Card card in handDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                // Debug.Log("[HandManager | Hand Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
            }
        }

        foreach (Card card in drawDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                // Debug.Log("[HandManager | Draw Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false); //disable and not notify
            }
        }

        foreach (Card card in discardDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                //Debug.Log("[HandManager | Discard Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false); //disable and not notify
            }
        }

        foreach (Card card in exhaustDeck.cards)
        {
            if (!listOfCardsOnHand.ContainsKey(card.id))
            {
                // Debug.Log("[HandManager | Exhaust Deck] Instantiating card " + card.id);
                GameObject newCard = Instantiate(spriteCardPrefab, this.transform);
                listOfCardsOnHand.Add(card.id, newCard);
                newCard.GetComponent<CardOnHandManager>().Populate(card, cardPilesData.data.energy);
                newCard.GetComponent<CardOnHandManager>().DisableCardContent(false); //disable and not notify
            }
        }

        // Debug.Log("[HandManager] listOfCardsOnHand.Count:" + listOfCardsOnHand.Count);

        StartCoroutine(RelocateCards(true));
    }

    private void RequestCardsPilesData()
    {
        if (!requestTimerIsRunning)
        {
            // if theres not a request running, start the coroutine
            requestTimer = StartCoroutine(WaitToRequestCardPiles());
        }
        else if (requestTimerIsRunning)
        {
            // if there is a request running, tell it to check again
            requestAgain = true;
        }
    }

    private IEnumerator WaitToRequestCardPiles()
    {
        do
        {
            Debug.Log("[HandManager] Card Pile Request Sent");
            // flag that the timer is running
            requestTimerIsRunning = true;
            // then make the request
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
            //if this fails, it will tell this function to check again
            Invoke(nameof(OnDrawCards), 0.2f);
            yield return new WaitForSeconds(1);
            // if there are still no cards after a second, make another request
        } while (requestAgain);

        // if the request was made, drop out
        requestTimerIsRunning = false;
    }

    /// <summary>
    /// Relocates the cards in hands. If move is on, card movement is send to the cards themselves to be preformed.
    /// </summary>
    /// <param name="move">True to do a draw animation to hand.</param>
    private IEnumerator RelocateCards(bool move = false)
    {
        float counter = 0;
        float depth = GameSettings.HAND_CARD_SPRITE_Z;
        float halfWidth = handDeck.cards.Count * GameSettings.HAND_CARD_GAP / 2;

        string result = handDeck.cards.Count % 2 == 0 ? "even" : "odd";

        //Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++handDeck.cards.Count=" + handDeck.cards.Count+" is "+ result);

        //float offset = handDeck.cards.Count % 2 == 0 ?  GameSettings.HAND_CARD_GAP / 2 : GameSettings.HAND_CARD_GAP/2;
        float offset = GameSettings.HAND_CARD_GAP / 2;
        float delayStep = 0.1f;
        float delay = delayStep * handDeck.cards.Count;

        // Debug.Log("----------------------------Relocate cards offset=" + offset);
        if (move)
        {
            bool pause = false;
            foreach (Card cardData in drawDeck.cards)
            {
                GameObject card;
                if (listOfCardsOnHand.TryGetValue(cardData.id, out card))
                {
                    var manager = card.GetComponent<CardOnHandManager>();

                    if (manager.TryMoveCardIfClose(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.draw,
                            out Sequence sequence))
                    {
                        // we can adjust the card right away here as this function is cleanup after other movement
                        if (sequence != null) sequence.Play();
                        yield return new WaitForSeconds(0.1f);
                        pause = true;
                    }
                }
            }

            foreach (Card cardData in handDeck.cards)
            {
                GameObject card;
                if (listOfCardsOnHand.TryGetValue(cardData.id, out card))
                {
                    var manager = card.GetComponent<CardOnHandManager>();
                    if (manager.TryMoveCardIfClose(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.draw,
                            out Sequence sequence))
                    {
                        // we can adjust the card right away here as this function is cleanup after other movement
                        if (sequence != null) sequence.Play();
                        yield return new WaitForSeconds(0.1f);
                        pause = true;
                    }
                }
            }

            if (pause)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        foreach (Card cardData in handDeck.cards)
        {
            GameObject card;
            if (listOfCardsOnHand.TryGetValue(cardData.id, out card))
            {
                var cardManager = card.GetComponent<CardOnHandManager>();

                Vector3 pos = Vector3.zero;
                pos.x = counter * GameSettings.HAND_CARD_GAP - halfWidth + offset;
                pos.y = GameSettings.HAND_CARD_REST_Y;
                pos.z = depth;

                var angle = (float)(pos.x * Mathf.PI * 2);
                pos.y += Mathf.Cos(pos.x * GameSettings.HAND_CARD_Y_CURVE);


                Vector3 rot = card.transform.eulerAngles;
                rot.z = angle / -2;
                card.transform.eulerAngles = rot;

                cardManager.targetRotation = rot;
                cardManager.targetPosition = pos;
                card.transform.localScale = Vector3.one;

                counter++;
                depth -= GameSettings.HAND_CARD_SPRITE_Z_INTERVAL;

                var manager = cardManager;
                if (move)
                {
                    // we can adjust the card right away here as this function is cleanup after other movement
                    manager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, pos, delay).Play();
                    delay -= delayStep;
                }
                else
                {
                    cardManager.TryResetPosition();
                }
            }
        }
    }

    private void OnCardsPilesUpdated(CardPiles data)
    {
        Debug.Log("[HandManager] OnCardPilesUpdated");
        cardPilesData = data;

        handDeck = new Deck();
        handDeck.cards = cardPilesData.data.hand;

        drawDeck = new Deck();
        drawDeck.cards = cardPilesData.data.draw;

        discardDeck = new Deck();
        discardDeck.cards = cardPilesData.data.discard;

        exhaustDeck = new Deck();
        exhaustDeck.cards = cardPilesData.data.exhausted;
    }
}