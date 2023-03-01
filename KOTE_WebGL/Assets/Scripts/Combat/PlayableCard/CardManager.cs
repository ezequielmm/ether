using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards
{
    public class CardManager : MonoBehaviour
    {
        public string id => cardData.id;

        internal Card cardData;
        [SerializeField] private bool _cardActive;
        internal bool card_can_be_played = true;
        internal bool hasUnplayableKeyword;
        internal CARDS_POSITIONS_TYPES currentPosition;
        
        // timer events
        private bool playResponseReceived;
        private bool timerRunning;
        
        // card modules
        private CardVisualsManager cardVisuals;
        private CardMovementManager cardMovement;
        
        internal bool cardActive
        {
            get => _cardActive;
            set
            {
                _cardActive = value;
                cardVisuals.UpdateCardBasedOnEnergy();
            }
        }
        
        private void Awake()
        {
            cardVisuals = GetComponent<CardVisualsManager>();
            cardMovement = GetComponent<CardMovementManager>();
            cardVisuals.DisableCardContent();
        }

        private void Start()
        {
            var death = gameObject.AddComponent<DestroyOnGameStatus>();
            death.causesOfDeath.Add(new DestroyOnGameStatus.CauseOfDeath()
            {
                UnParent = true,
                StatusToListenTo = GameStatuses.GameOver,
                AnimationTime = 1f,
                ShrinkToDie = true
            });
            GameManager.Instance.EVENT_CARD_PLAYED.AddListener(StartTimeout);
            GameManager.Instance.EVENT_MOVE_CARDS.AddListener(OnMoveCards);
        }
        
        public void SetCardPosition(Vector3 targetPosition, Vector3 targetRotation)
        {
            cardMovement.targetPosition = targetPosition;
            cardMovement.targetRotation = targetRotation;
        }

        public Sequence MoveCard(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
            Vector3 pos = default(Vector3), float moveDelay = 0, Sequence sequence = null, float sequenceStartPoint = 0)
        {
            return cardMovement.MoveCard(originType, destinationType, pos, moveDelay, sequence, sequenceStartPoint);
        }

        public bool TryMoveCardIfClose(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
            out Sequence sequence)
        {
           return cardMovement.TryMoveCardIfClose(originType, destinationType, out sequence);
        }

        public Sequence OnCardToMove(CardToMoveData data, float delayIndex)
        {
            return cardMovement.OnCardToMove(data, delayIndex);
        }

        public void TryResetPosition()
        {
            cardMovement.TryResetPosition();
        }

        public void Populate(Card card, int energy, Vector3[] pileOrthoPositions)
        {
            cardData = card;
            cardVisuals.Populate(card, energy);
            cardMovement.SetOrthoPositions(pileOrthoPositions);
        }

        public void DisableCardContent()
        {
            cardVisuals.DisableCardContent();
        }

        internal void ResetCardPosition()
        {
            cardMovement.ResetCardPosition();
        }

        internal void ShowUpCard()
        {
            cardMovement.ShowUpCard();
        }

        internal void ShowPointer()
        {
            cardMovement.ShowPointer();
        }

        internal void FollowMouse()
        {
            cardMovement.FollowMouse();
        }

        internal void StartTimeout(string cardId, string enemyId)
        {
            if(id == cardId) StartCoroutine(CardTimeout());
        }

        private void OnMoveCards(List<(CardToMoveData, float)> cards)
        {
            if (timerRunning && cards.Exists(x => x.Item1.id == id))
            {
                playResponseReceived = true;
            }
        }

        private IEnumerator CardTimeout()
        {
            timerRunning = true;
            yield return new WaitForSeconds(GameSettings.CARD_PLAYED_TIMEOUT_DELAY);

            if (!playResponseReceived)
            {
                Debug.LogWarning($"Warning! No move response after playing card {id}. Discarding card.");
                cardMovement.MoveCard(currentPosition, CARDS_POSITIONS_TYPES.discard).Play();
            }
            timerRunning = false;
        }
    }
}