using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CardManagement
{
    public class CardPilesManager : MonoBehaviour
    {
        public CardOnHandManager spriteCardPrefab;
        public DiscardPileManager discardManager;
        public ExhaustPileManager exhaustManager;
        public HandManager handManager;

        public DrawPileManager drawManager;

        internal Dictionary<string, CardOnHandManager>
            MasterCardList = new Dictionary<string, CardOnHandManager>(); // CardID <--> Card Obj

        private CardPiles cardPilesData;

        private bool gameOver;

        private bool requestTimerIsRunning;
        private bool requestAgain;
        private Coroutine requestTimer;


        // Start is called before the first frame update
        void Start()
        {
            GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(OnDrawCards);
            GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnCardsPilesUpdated);
            GameManager.Instance.EVENT_CARD_ADD.AddListener(OnCardAdd);
            GameManager.Instance.EVENT_CARD_DISABLED.AddListener(OnCardDestroyed);
            GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameOver);
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

            // create card lists
            handManager.handDeck.Clear();
            drawManager.drawDeck.Clear();
            discardManager.discardDeck.Clear();
            exhaustManager.exhaustDeck.Clear();

            float delayStep = 0.1f;

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

            // Debug.Log("[HandManager] listOfCardsOnHand.Count:" + listOfCardsOnHand.Count);
            StartCoroutine(ValidateCardPositions());
        }

        private void SpawnCardToPile(List<CardOnHandManager> cardPile, Card card)
        {
            if (!MasterCardList.ContainsKey(card.id))
            {
               SpawnCard(cardPile, card);
            }
            else
            {
                cardPile.Add(MasterCardList[card.id]);
            }
        }

        private void SpawnCard(List<CardOnHandManager> cardPile, Card card)
        {
            // Debug.Log("[HandManager | Hand Deck] Instantiating card " + card.id);
            CardOnHandManager newCard = Instantiate(spriteCardPrefab, handManager.transform);
            newCard.Populate(card, cardPilesData.data.energy);
            MasterCardList.Add(card.id, newCard);
            cardPile.Add(newCard);
        }

        private IEnumerator ValidateCardPositions()
        {
            // Debug.Log("----------------------------Relocate cards offset=" + offset);
            bool pause = false;
            foreach (CardOnHandManager cardData in drawManager.drawDeck)
            {
                if (cardData.TryMoveCardIfClose(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.draw,
                        out Sequence sequence))
                {
                    // we can adjust the card right away here as this function is cleanup after other movement
                    if (sequence != null) sequence.Play();
                    yield return new WaitForSeconds(0.1f);
                    pause = true;
                }
            }

            foreach (CardOnHandManager cardManager in handManager.handDeck)
            {
                if (cardManager.TryMoveCardIfClose(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.draw,
                        out Sequence sequence))
                {
                    // we can adjust the card right away here as this function is cleanup after other movement
                    if (sequence != null) sequence.Play();
                    yield return new WaitForSeconds(0.1f);
                    pause = true;
                }
            }

            if (pause)
            {
                yield return new WaitForSeconds(1f);
            }

            handManager.StartRelocateCards(true);
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

        private void OnCardAdd(AddCardData addCardData)
        {
            // create the new card in the middle of the screen
            CardOnHandManager newCard = Instantiate(spriteCardPrefab, this.transform);
            MasterCardList.Add(addCardData.card.id, newCard);
            newCard.Populate(addCardData.card, cardPilesData.data.energy);
            newCard.ShowAddedCard(addCardData);
        }

        private void OnCardsPilesUpdated(CardPiles data)
        {
            Debug.Log("[HandManager] OnCardPilesUpdated");
            cardPilesData = data;

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

        private void VerifyCardPosition(Card card, CARDS_POSITIONS_TYPES position, List<CardOnHandManager> cardPile)
        {
            if (!ConfirmCardIsInPile(card, position))
            {
                if (!MasterCardList.ContainsKey(card.id))
                {
                    SpawnCard(cardPile, card);
                }
                else
                {
                    CardOnHandManager curCard = MasterCardList[card.id];
                    curCard.MoveCard(curCard.currentPosition, position);
                }
            }
        }

        private bool ConfirmCardIsInPile(Card card, CARDS_POSITIONS_TYPES position)
        {
            if (MasterCardList.ContainsKey(card.id) && MasterCardList[card.id].currentPosition == position)
            {
                return true;
            }

            {
                return false;
            }
        }


        private void OnCardDestroyed(string cardId)
        {
            Debug.Log("[HandManager] Removing card " + cardId + " from hand");

            var cardMoved = handManager.handDeck.Find(card => card.cardData.id == cardId);
            if (cardMoved != null)
            {
                handManager.handDeck.Remove(cardMoved);
                discardManager.discardDeck.Add(cardMoved);
            }

            handManager.StartRelocateCards();
        }
    }
}