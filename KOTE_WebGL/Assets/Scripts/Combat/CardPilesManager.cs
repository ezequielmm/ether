using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards.Piles
{
    public class CardPilesManager : MonoBehaviour
    {
        public CardAnimationQueue animationQueue;
        public CardManager SpriteCardPrefab;
        public DiscardPileManager discardManager;
        public ExhaustPileManager exhaustManager;
        public HandManager handManager;

        public DrawPileManager drawManager;

        internal Dictionary<string, CardManager>
            MasterCardList = new Dictionary<string, CardManager>(); // CardID <--> Card Obj

        private CardPiles cardPilesData;

        private bool gameOver;

        private bool requestTimerIsRunning;
        private bool requestAgain;
        private Coroutine requestTimer;

        // this is a field due to abstraction
        private bool pause;

        /*
         * A list of ortho positions to pass on to the card, so every card made doesn't pull them
         * [draw, discard, exhaust]
         */
        private Vector3[] pileOrthoPositionArray = new Vector3[3];


        // Start is called before the first frame update
        void Start()
        {
            GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
            GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
            GameManager.Instance.EVENT_CARD_ADD.AddListener(GameplayCardSpawn);
            GameManager.Instance.EVENT_CARD_DISABLED.AddListener(OnCardDestroyed);
            GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameOver);
            
            pileOrthoPositionArray[0] = TransformUIToOrtho(drawManager.gameObject);
            pileOrthoPositionArray[1] = TransformUIToOrtho(discardManager.gameObject);
            pileOrthoPositionArray[2] = TransformUIToOrtho(exhaustManager.gameObject);
        }

        private void OnGameOver(GameStatuses gameStatus)
        {
            if (gameStatus == GameStatuses.GameOver)
            {
                gameOver = true;
            }
        }

        private void OnDrawCards()
        {
            // if the game is over stop asking for cards
            if (gameOver)
            {
                requestAgain = false;
                return;
            }

            Debug.Log("[CardPilesManager]------------------------------------------------>[OnDrawCards]");
            if (CardPilesDoesNotHaveHandData())
            {
                Debug.Log(
                    $"[CardPilesManager] Insufficient card data to draw cards. Hand Count is {cardPilesData?.data?.hand?.Count} Requesting Card Piles");
                RequestCardsPilesData();
                return;
            }

            Debug.Log(
                $"[CardPilesManager] Card Piles Retrieved. draw.count: {cardPilesData.data.draw.Count} | hand.count: {cardPilesData.data.hand.Count} | discard.count: {cardPilesData.data.discard.Count} | exhaust.count: {cardPilesData.data.exhausted.Count}");
            requestAgain = false;
            
            // Debug.Log("[CardPilesManager] listOfCardsOnHand.Count:" + listOfCardsOnHand.Count);
            StartCoroutine(ConfirmCardsAreInDrawPile());
        }

        private bool CardPilesDoesNotHaveHandData()
        {
            return cardPilesData == null || cardPilesData.data.hand.Count < 1;
        }

        private void ClearCardLists()
        {
            handManager.handDeck.Clear();
            drawManager.drawDeck.Clear();
            discardManager.discardDeck.Clear();
            exhaustManager.exhaustDeck.Clear();
        }

        private void PopulateCardLists()
        {
            foreach (Card card in cardPilesData.data.hand)
            {
                SpawnCardToPile(handManager.handDeck, card);
            }

            foreach (Card card in cardPilesData.data.draw)
            {
                SpawnCardToPile(drawManager.drawDeck, card);
            }

            foreach (Card card in cardPilesData.data.discard)
            {
                SpawnCardToPile(discardManager.discardDeck, card);
            }

            foreach (Card card in cardPilesData.data.exhausted)
            {
                SpawnCardToPile(exhaustManager.exhaustDeck, card);
            }
        }

        private void SpawnCardToPile(List<CardManager> cardPile, Card card)
        {
            if (!MasterCardList.ContainsKey(card.id))
            {
                InitialCardSpawn(cardPile, card);
            }
            else
            {
                cardPile.Add(MasterCardList[card.id]);
            }
        }
        
        private void OnCardsPilesUpdated(CardPiles data)
        {
            Debug.Log("[CardPilesManager] OnCardPilesUpdated");
            cardPilesData = data;
            
            ClearCardLists();
            PopulateCardLists();

            foreach (Card card in data.data.draw)
            {
                VerifyCardPosition(card, CARDS_POSITIONS_TYPES.draw, drawManager.drawDeck);
            }

            foreach (Card card in data.data.hand)
            {
                VerifyCardPosition(card, CARDS_POSITIONS_TYPES.hand, handManager.handDeck);
            }

            foreach (Card card in data.data.discard)
            {
                VerifyCardPosition(card, CARDS_POSITIONS_TYPES.discard, discardManager.discardDeck);
            }

            foreach (Card card in data.data.exhausted)
            {
                VerifyCardPosition(card, CARDS_POSITIONS_TYPES.exhaust, exhaustManager.exhaustDeck);
            }
        }

        private void VerifyCardPosition(Card card, CARDS_POSITIONS_TYPES position, List<CardManager> cardPile)
        {
            if (!ConfirmCardIsInPile(card, cardPile))
            {
                if (!MasterCardList.ContainsKey(card.id))
                {
                    InitialCardSpawn(cardPile, card);
                }
                else
                {
                    CardManager cardManager = MasterCardList[card.id];
                    animationQueue.AddSequenceToQueue(cardManager.MoveCard(cardManager.currentPosition, position));
                }
            }
        }

        private bool ConfirmCardIsInPile(Card card, List<CardManager> cardPile)
        {
            if (MasterCardList.ContainsKey(card.id) && cardPile.Exists(x => x.cardData.id == card.id))
            {
                return true;
            }

            return false;
        }

        // called when creating cards at start of combat
        private void InitialCardSpawn(List<CardManager> cardPile, Card card)
        {
            // Debug.Log("[CardPilesManager | Hand Deck] Instantiating card " + card.id);
            CardManager cardManager = SpawnCard(card);
            if (cardPile != handManager.handDeck) cardManager.DisableCardContent();
            cardPile.Add(cardManager);
        }

        // called when a card is created in the midst of combat (typically status cards)
        private void GameplayCardSpawn(AddCardData addCardData)
        {
            // create the new card in the middle of the screen
            SpawnCard(addCardData.card);
            GameManager.Instance.EVENT_MOVE_CARDS.Invoke(new List<(CardToMoveData, float)>
            {
                (new CardToMoveData
                {
                    destination = addCardData.destination,
                    id = addCardData.card.id,
                    source = "none"
                }, GameSettings.SHOW_NEW_CARD_DURATION)
            });
        }

        private CardManager SpawnCard(Card card)
        {
            CardManager cardManager = Instantiate(SpriteCardPrefab, handManager.transform);
            cardManager.gameObject.name = card.name + " " + card.id;
            cardManager.Populate(card, cardPilesData.data.energy, pileOrthoPositionArray);
            MasterCardList.Add(card.id, cardManager);
            return cardManager;
        }

        private IEnumerator ConfirmCardsAreInDrawPile()
        {
            // Debug.Log("----------------------------Relocate cards offset=" + offset);
            pause = false;
            foreach (CardManager cardManager in drawManager.drawDeck)
            {
                yield return ConfirmCardInDrawPile(cardManager);
            }

            foreach (CardManager cardManager in handManager.handDeck)
            {
                yield return ConfirmCardInDrawPile(cardManager);
            }

            if (pause)
            {
                yield return new WaitForSeconds(1f);
            }

            handManager.StartRelocateCards(true);
            pause = false;
        }

        private IEnumerator ConfirmCardInDrawPile(CardManager cardManager)
        {
            if (cardManager.TryMoveCardIfClose(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.draw,
                    out Sequence sequence))
            {
                // we can adjust the card right away here as this function is cleanup after other movement
                if (sequence != null) sequence.Play();
                yield return new WaitForSeconds(0.1f);
                pause = true;
            }

            yield return null;
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
                Debug.Log("[CardPilesManager] Card Pile Request Sent");
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

      


        private void OnCardDestroyed(string cardId)
        {
            Debug.Log("[CardPilesManager] Removing card " + cardId + " from hand");

            var cardMoved = handManager.handDeck.Find(card => card.cardData.id == cardId);
            if (cardMoved != null)
            {
                handManager.handDeck.Remove(cardMoved);
                discardManager.discardDeck.Add(cardMoved);
            }

            handManager.StartRelocateCards();
        }

        private Vector3 TransformUIToOrtho(GameObject cardPile)
        {
            Vector3 pos = cardPile.transform.position; //(1.1, 104.5, 0.0)

            float height = 2 * Camera.main.orthographicSize; //10
            float width = height * Camera.main.aspect; //21.42

            //transform UI coordinates to orthorgraphic coordinates
            float xx = pos.x * width / Screen.width;
            xx -= width / 2; //ortho counts from the center 
            float yy = pos.y * height / Screen.height;
            yy -= height / 2;

            return new Vector3(xx, yy, handManager.transform.position.z);
        }
    }
}