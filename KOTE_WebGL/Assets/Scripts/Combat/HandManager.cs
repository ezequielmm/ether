using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards.Piles
{
    public class HandManager : MonoBehaviour
    {
         public List<CardManager> handDeck = new();

        int cardsDrawn = 0;
        bool audioRunning = false;

        private void Awake()
        {
            GameManager.Instance.EVENT_CARD_DRAW.AddListener(OnCardDraw); // SFX
            // if we're adding a card to the hand that isn't a draw
            GameManager.Instance.EVENT_REARRANGE_HAND.AddListener(OnRearrangeHand);
            // if the game is over, don't try to draw more cards
        }

        private void OnCardDraw()
        {
            //Debug.Log($"[Hand Pile] Card Drawn.");
            cardsDrawn++;
            StartCoroutine(DrawCardSfx());
        }


        private IEnumerator DrawCardSfx()
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

        private void OnRearrangeHand()
        {
            RelocateCards();
        }

        internal void StartRelocateCards(bool drawCards = false)
        {
            RelocateCards(drawCards);
        }


        /// <summary>
        /// Relocates the cards in hand. If move is on, card movement is send to the cards themselves to be preformed.
        /// </summary>
        /// <param name="drawCards">True to do a draw animation to hand.</param>
        private void RelocateCards(bool drawCards = false)
        {
            float counter = 0;
            float depth = GameSettings.HAND_CARD_SPRITE_Z;
            float halfWidth = handDeck.Count * GameSettings.HAND_CARD_GAP / 2;

            string result = handDeck.Count % 2 == 0 ? "even" : "odd";

            //Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++handDeck.cards.Count=" + handDeck.cards.Count+" is "+ result);

            float offset = GameSettings.HAND_CARD_GAP / 2;

            float delayStep = 0.1f;
            float delay = delayStep * handDeck.Count;

            foreach (CardManager cardManager in handDeck)
            {
                GameObject card = cardManager.gameObject;

                Vector3 pos = Vector3.zero;
                pos.x = counter * GameSettings.HAND_CARD_GAP - halfWidth + offset;
                pos.y = GameSettings.HAND_CARD_REST_Y;
                pos.z = depth;

                var angle = (float)(pos.x * Mathf.PI * 2);
                pos.y += Mathf.Cos(pos.x * GameSettings.HAND_CARD_Y_CURVE);


                Vector3 rot = card.transform.eulerAngles;
                rot.z = angle / -2;
                card.transform.eulerAngles = rot;

                cardManager.SetCardPosition(pos, rot);
                card.transform.localScale = Vector3.one;

                counter++;
                depth -= GameSettings.HAND_CARD_SPRITE_Z_INTERVAL;

                if (drawCards)
                {
                    // we can adjust the card right away here as this function is cleanup after other movement
                    cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, pos, delay).Play();
                    delay -= delayStep;
                }
                else
                {
                    cardManager.TryResetPosition();
                }
            }
        }
    }
}