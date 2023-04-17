using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards
{
    public class CardMovementManager : MonoBehaviour
    {
        public CardManager cardManager;
        public CardVisualsManager visualsManager;
        private Vector3 drawPileOrthoPosition;
        private Vector3 discardPileOrthoPosition;
        private Vector3 exhaustPileOrthoPosition;
        static float delay;
        new Collider2D collider;

        private bool activateCardAfterMove;
        private bool discardAfterMove;

        internal Vector3 targetPosition;
        internal Vector3 targetRotation;

        private TargetProfile targetProfile;


        private void Awake()
        {
            cardManager.inTransit = false;
        }

        private void Start()
        {
            collider = GetComponent<Collider2D>();
            GameManager.Instance.EVENT_CARD_SHOWING_UP.AddListener(OnCardMouseShowingUp);
            targetProfile = new TargetProfile()
            {
                player = false,
                enemy = true
            };
        }

        private void Update()
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                if (delay < 0) delay = 0;
            }
        }

        internal void SetOrthoPositions(Vector3[] cardPilePositions)
        {
            drawPileOrthoPosition = cardPilePositions[0];
            discardPileOrthoPosition = cardPilePositions[1];
            exhaustPileOrthoPosition = cardPilePositions[2];
        }
        
        
        internal void ShowPointer()
        {
            //show the pointer instead of following the mouse
            PointerData pd = new PointerData(transform.position, PointerOrigin.card, targetProfile);

            GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(pd);
            GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(false);

            Vector3 showUpPosition =
                new Vector3(0, GameSettings.HAND_CARD_SHOW_UP_Y, GameSettings.HAND_CARD_SHOW_UP_Z);
            transform.DOMove(showUpPosition, GameSettings.HAND_CARD_SHOW_UP_TIME);
        }

        internal void FollowMouse()
        {
            float zz = transform.position.z;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = zz;
            transform.position = mousePos;
        }

        internal Sequence OnCardToMove(CardToMoveData data, float delayIndex)
        {
            Sequence returnSequence = null;
            if (cardManager.cardData.id == data.id)
            {
                Enum.TryParse(data.source, out CARDS_POSITIONS_TYPES origin);
                Enum.TryParse(data.destination, out CARDS_POSITIONS_TYPES destination);
                if (origin == CARDS_POSITIONS_TYPES.discard)
                {
                    delay += 0.1f;
                }

                if (!(origin == CARDS_POSITIONS_TYPES.draw && destination == CARDS_POSITIONS_TYPES.hand) &&
                    origin != CARDS_POSITIONS_TYPES.none)
                {
                    delayIndex = 0;
                }

                float internalDelay = 0;
                if (delayIndex > 0)
                {
                    internalDelay += 1;
                }

                if (cardManager.inTransit)
                {
                    delay += 1.1f;
                }

                if (delay > 0 || delayIndex > 0)
                {
                    returnSequence = MoveAfterTime(
                        delay + (delayIndex * GameSettings.CARD_DRAW_SHOW_TIME) + internalDelay,
                        origin, destination);
                }
                else
                {
                    returnSequence = MoveCard(origin, destination);
                }

                if (data.card != null && data.card.name != null) visualsManager.UpdateCardEnergyText(data.card.energy);
            }

            return returnSequence;
        }

        private Sequence MoveAfterTime(float moveDelay, CARDS_POSITIONS_TYPES origin, CARDS_POSITIONS_TYPES destination)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Rewind();
            sequence.PrependInterval(moveDelay);
            return MoveCard(origin, destination, sequence: sequence, sequenceStartPoint: moveDelay);
        }

        internal bool TryMoveCardIfClose(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
            out Sequence sequence)
        {
            Vector3 origin = new Vector3();
            switch (originType)
            {
                case CARDS_POSITIONS_TYPES.draw:
                    origin = drawPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.discard:
                    origin = discardPileOrthoPosition;
                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    origin = this.transform.position;
                    break;
                case CARDS_POSITIONS_TYPES.exhausted:
                    origin = exhaustPileOrthoPosition;
                    break;
            }

            if (Mathf.Abs((this.transform.position - origin).magnitude) <= 0.1f)
            {
                sequence = MoveCard(originType, destinationType);
                return true;
            }

            sequence = null;
            // Debug.Log($"[CardOnHandManager] Card {thisCardValues.id} is not from {originType} and will not be moved to {destinationType}.");
            return false;
        }


        public Sequence MoveCard(CARDS_POSITIONS_TYPES originType, CARDS_POSITIONS_TYPES destinationType,
            Vector3 pos = default(Vector3), float moveDelay = 0, Sequence sequence = null, float sequenceStartPoint = 0)
        {
            if (sequence == null)
            {
                sequence = DOTween.Sequence();
                sequence.Rewind();
            }

            if (originType == destinationType) return sequence;

            Debug.Log("[CardOnHandManager] MoveCard = " + originType + " to " + destinationType + "........card id: " +
                      cardManager.cardData.id);
            visualsManager.PlayCardParticles(CARD_PARTICLE_TYPES.Move);

            //   Debug.Log("[CardOnHandManager] Card Is Now Moving");

            Vector3 origin = new Vector3();
            Vector3 destination = new Vector3();

            switch (originType)
            {
                case CARDS_POSITIONS_TYPES.draw:
                    origin = drawPileOrthoPosition;
                    transform.localScale = Vector3.zero;
                    break;
                case CARDS_POSITIONS_TYPES.discard:
                    origin = discardPileOrthoPosition;
                    transform.localScale = Vector3.one * 0.2f;
                    break;
                case CARDS_POSITIONS_TYPES.hand:
                    origin = this.transform.position;
                    break;
                case CARDS_POSITIONS_TYPES.exhausted:
                    origin = exhaustPileOrthoPosition;
                    transform.localScale = Vector3.one * 0.2f;
                    break;
                case CARDS_POSITIONS_TYPES.none:
                    origin = Vector3.zero;
                    break;
            }

            activateCardAfterMove = false;

            cardManager.currentPosition = destinationType;

            if (pos.magnitude > 0)
            {
                destination = pos;
                activateCardAfterMove = true;
            }
            else
            {
                switch (destinationType)
                {
                    case CARDS_POSITIONS_TYPES.draw:
                        destination = drawPileOrthoPosition;
                        break;
                    case CARDS_POSITIONS_TYPES.discard:
                        destination = discardPileOrthoPosition;
                        if (originType == CARDS_POSITIONS_TYPES.hand)
                        {
                            discardAfterMove = true;
                        }

                        break;
                    case CARDS_POSITIONS_TYPES.hand:
                        destination = pos;
                        destination.z = GameSettings.HAND_CARD_SHOW_UP_Z;
                        activateCardAfterMove = true;
                        break;
                    case CARDS_POSITIONS_TYPES.exhausted:
                        destination = Vector2.zero;
                        destination.z = GameSettings.HAND_CARD_SHOW_UP_Z;
                        discardAfterMove = true;
                        break;
                }
            }

            if (originType != destinationType)
            {
                switch (destinationType)
                {
                    case CARDS_POSITIONS_TYPES.draw:
                        GameManager.Instance.EVENT_CARD_SHUFFLE.Invoke();
                        break;
                    case CARDS_POSITIONS_TYPES.discard:
                        GameManager.Instance.EVENT_CARD_DISCARD.Invoke();
                        break;
                    case CARDS_POSITIONS_TYPES.hand:
                        GameManager.Instance.EVENT_CARD_DRAW.Invoke();
                        break;
                    case CARDS_POSITIONS_TYPES.exhausted:
                        GameManager.Instance.EVENT_CARD_EXHAUST.Invoke();
                        break;
                }
            }

            cardManager.cardActive = false;
            if (destinationType == CARDS_POSITIONS_TYPES.hand) visualsManager.EnableCardContent();

            if (moveDelay > 0)
            {
                cardManager.inTransit = true;
                sequence.Insert(sequenceStartPoint,
                    transform.DOMove(destination, 1f).SetDelay(moveDelay, true).SetEase(Ease.InCirc).From(origin));
                if (destinationType == CARDS_POSITIONS_TYPES.hand)
                {
                    transform.localScale = Vector3.zero;
                    sequence.Insert(sequenceStartPoint, transform.DOScale(Vector3.one, 1f).SetDelay(moveDelay, true)
                        .SetEase(Ease.OutElastic)
                        .OnComplete(OnMovingToHandCompleted));
                }
                else
                {
                    sequence.Insert(sequenceStartPoint, transform.DOScale(Vector3.zero, 1f).SetDelay(moveDelay, true)
                        .SetEase(Ease.InElastic)
                        .OnComplete(HideAndDeactivateCard));
                }
            }
            else
            {
                cardManager.inTransit = true;
                sequence.Insert(sequenceStartPoint,
                    transform.DOMove(destination, 1f).From(origin).SetEase(Ease.OutCirc));
                if (destinationType == CARDS_POSITIONS_TYPES.hand)
                {
                    Debug.Log("[CardOnHandManager] Draw new card to hand.");
                    transform.localScale = Vector3.zero;
                    sequence.Insert(sequenceStartPoint, transform.DORotate(Vector3.zero, 1f));
                    sequence.Insert(sequenceStartPoint,
                        transform.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.OutElastic)
                            .OnComplete(OnMovingToHandCompleted));
                }
                else
                {
                    if (destinationType == CARDS_POSITIONS_TYPES.exhausted)
                    {
                        visualsManager.StopCardParticles(CARD_PARTICLE_TYPES.Move);
                        cardManager.cardActive = false;
                        sequence.Insert(sequenceStartPoint,
                            transform.DOScale(Vector3.one * 1.5f, 1f).SetEase(Ease.InOutQuad)
                                .OnComplete(HideAndDeactivateCard));
                        sequence.Insert(sequenceStartPoint,
                            transform.DORotate(Vector3.zero, 1f).SetEase(Ease.InOutQuad));
                    }
                    else
                    {
                        sequence.Insert(sequenceStartPoint,
                            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InElastic)
                                .OnComplete(HideAndDeactivateCard));
                    }
                }
            }
            return sequence;
        }

        private void OnMovingToHandCompleted()
        {
            cardManager.inTransit = false;
            cardManager.cardActive = activateCardAfterMove;
            visualsManager.StopCardParticles(CARD_PARTICLE_TYPES.Move);

            if (cardManager.cardActive && ((Vector2)transform.position).magnitude < 0.5f)
                // if in the center of the screen
            {
                cardManager.inTransit = true;
                cardManager.cardActive = false;
                StartCoroutine(ResetAfterTime(GameSettings.CARD_DRAW_SHOW_TIME));
            }

            if (cardManager.cardActive)
            {
                visualsManager.UpdateCardBasedOnEnergy();
            }
        }

        private void HideAndDeactivateCard()
        {
            if (cardManager.currentPosition == CARDS_POSITIONS_TYPES.hand) return;
            visualsManager.StopCardParticles(CARD_PARTICLE_TYPES.Move);

            cardManager.inTransit = false;

            if (cardManager.currentPosition == CARDS_POSITIONS_TYPES.exhausted)
            {
                StartCoroutine(ExhaustEffect());
                return;
            }

            if (discardAfterMove)
            {
                discardAfterMove = false;
                GameManager.Instance.EVENT_CARD_DISABLED.Invoke(cardManager.cardData.id);
            }

            if (!activateCardAfterMove)
            {
                visualsManager.DisableCardContent();
            }
            else
            {
                cardManager.cardActive = true;
            }
        }

        private IEnumerator ExhaustEffect()
        {
            yield return null;

            float effectLength = GameSettings.EXHAUST_EFFECT_DURATION;

            // Temp fade animation
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.DOFade(0, effectLength);
            }

            foreach (var tmp in GetComponentsInChildren<TextMeshPro>())
            {
                tmp.DOFade(0, effectLength);
            }

            yield return new WaitForSeconds(effectLength);

            // Hide and deactivate card
            discardAfterMove = false;
            GameManager.Instance.EVENT_CARD_DISABLED.Invoke(cardManager.cardData.id);
            visualsManager.DisableCardContent();

            // move cart to end position
            transform.position = discardPileOrthoPosition;
        }

        IEnumerator ResetAfterTime(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            cardManager.inTransit = false;
            cardManager.cardActive = true;
            ResetCardPosition();
        }

        internal void ResetCardPosition()
        {
            if (cardManager.cardActive)
            {
                // Debug.Log("[ResetCardPosition]");
                visualsManager.StopCardParticles(CARD_PARTICLE_TYPES.Aura);

                DOTween.Kill(this.transform);
                // make sure we rearrange the hand after the card is reset so it looks nice
                transform.DOMove(targetPosition, GameSettings.HAND_CARD_RESET_POSITION_TIME);
                transform.DOScale(Vector3.one, GameSettings.HAND_CARD_RESET_POSITION_TIME);
                transform.DORotate(targetRotation, GameSettings.HAND_CARD_RESET_POSITION_TIME);
            }
        }

        internal void TryResetPosition()
        {
            if (cardManager.inTransit)
            {
                StartCoroutine(TryResetAfterTime(0.25f));
            }
            else
            {
                ResetCardPosition();
            }
        }

        IEnumerator TryResetAfterTime(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            TryResetPosition();
        }


        private void OnCardMouseShowingUp(string cardId, Vector3 cardPos)
        {
            if (cardId != cardManager.cardData.id && cardManager.cardActive)
            {
                // Debug.Log("Check mouse is left or right "+ TransformMouseToOrtho().x+"/"+this.transform.position.x);            

                float direction = cardPos.x > this.transform.position.x ? -0.5f : 0.5f;

                this.transform.DOMoveX(targetPosition.x + direction, 0.5f);
            }
        }

        internal void ShowUpCard()
        {
            if (cardManager.cardActive)
            {
                ResetCardPosition();
                DOTween.Kill(this.transform);

                if (cardManager.card_can_be_played) visualsManager.PlayCardParticles(CARD_PARTICLE_TYPES.Aura);

                //  Debug.Log("ShowUp");
                transform.DOScale(Vector3.one * GameSettings.HAND_CARD_SHOW_UP_SCALE,
                    GameSettings.HAND_CARD_SHOW_UP_TIME);


                Vector3 showUpPosition = new Vector3(targetPosition.x, GameSettings.HAND_CARD_SHOW_UP_Y,
                    GameSettings.HAND_CARD_SHOW_UP_Z);
                transform.DOMove(showUpPosition, GameSettings.HAND_CARD_SHOW_UP_TIME);


                transform.DORotate(Vector3.zero, GameSettings.HAND_CARD_SHOW_UP_TIME).OnComplete(() =>
                {
                    if (transform.position.x <= 0)
                    {
                        Vector3 topRightOfCard = new Vector3(transform.position.x + (collider.bounds.size.x / 2) + 0.2f,
                            transform.position.y + (collider.bounds.size.y / 2), 0);
                        visualsManager.SetTooltips(topRightOfCard, TooltipController.Anchor.TopLeft);
                    }
                    else
                    {
                        Vector3 topLeftOfCard = new Vector3(
                            transform.position.x - ((collider.bounds.size.x / 2) + 0.2f),
                            transform.position.y + (collider.bounds.size.y / 2), 0);
                        visualsManager.SetTooltips(topLeftOfCard, TooltipController.Anchor.TopRight);
                    }
                });

                GameManager.Instance.EVENT_CARD_SHOWING_UP.Invoke(cardManager.cardData.id, this.targetPosition);
            }
            else
            {
                visualsManager.StopCardParticles(CARD_PARTICLE_TYPES.Aura);
            }
        }
    }
}