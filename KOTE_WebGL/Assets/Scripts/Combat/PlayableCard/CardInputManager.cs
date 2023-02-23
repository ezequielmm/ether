using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace KOTE.Expedition.Combat.Cards
{
    public class CardInputManager : MonoBehaviour
    {
        public CardManager cardManager;
        private bool cardBeingDragged;
        private bool pointerIsActive;
        private bool cardIsDisplaced;
        private bool cardIsShowingUp;
        private bool awaitMouseUp;
        

        private bool overPlayer;
        private GameObject lastOver;
        private TargetProfile targetProfile;

        private void Start()
        {
            GameManager.Instance.EVENT_CARD_MOUSE_EXIT.AddListener(OnCardMouseExit);
            targetProfile = new TargetProfile()
            {
                player = false,
                enemy = true
            };
        }

        private void Update()
        {
            if (awaitMouseUp && !Input.GetMouseButton(0) && !cardBeingDragged)
            {
                awaitMouseUp = false;
                cardIsShowingUp = false;
                cardIsDisplaced = false;
                cardManager.ResetCardPosition();
            }
        }

        private void OnMouseEnter()
        {
            if (cardManager.cardActive && !Input.GetMouseButton(0) && !cardIsShowingUp)
            {
                cardIsShowingUp = true;
                cardManager.ShowUpCard();
            }
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonUp(0) && !cardIsShowingUp)
            {
                cardIsShowingUp = true;
                cardManager.ShowUpCard();
            }
        }

        private void OnMouseDrag()
        {
            cardBeingDragged = true;
            if (!cardManager.card_can_be_played)
            {
                return;
            }

            if (!cardManager.cardActive) 
            {
                //TODO: show no energy message
                return;
            }

            // Debug.Log("Distance y is " + xxDelta);
            if (cardManager.cardData != null && cardManager.cardData.showPointer)
            {
                //show the pointer instead of following the mouse
                PointerData pd = new PointerData(transform.position, PointerOrigin.card, targetProfile);

                GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(pd);
                GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(false);

                Vector3 showUpPosition =
                    new Vector3(0, GameSettings.HAND_CARD_SHOW_UP_Y, GameSettings.HAND_CARD_SHOW_UP_Z);
                transform.DOMove(showUpPosition, GameSettings.HAND_CARD_SHOW_UP_TIME);

                pointerIsActive = true;
                return;
            }

            float zz = transform.position.z;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = zz;
            transform.position = mousePos;
        }

        private void OnMouseExit()
        {
            GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();

            if (pointerIsActive) return;

            if (cardManager.cardActive && cardIsShowingUp)
            {
                // Debug.Log("[OnMouseExit]");
                GameManager.Instance.EVENT_CARD_MOUSE_EXIT.Invoke(cardManager.cardData.id);

                if (cardIsDisplaced)
                {
                    // Play Cancellation sound
                    GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Cancel");
                }

                if (!Input.GetMouseButton(0) && !cardBeingDragged)
                {
                    cardIsShowingUp = false;
                    cardIsDisplaced = false;
                    cardManager.ResetCardPosition();
                }
                else
                    awaitMouseUp = true;
            }
        }

        private void OnMouseDown()
        {
            if (!cardManager.card_can_be_played && !cardManager.hasUnplayableKeyword)
            {
                GameManager.Instance.EVENT_CARD_NO_ENERGY.Invoke();
            }
        }

        private void OnMouseUp()
        {
            if (pointerIsActive)
            {
                cardIsShowingUp = false;
                cardIsDisplaced = false;
                cardManager.ResetCardPosition();
                pointerIsActive = false;
            }

            if (cardManager.cardData.showPointer)
            {
                GameManager.Instance.EVENT_DEACTIVATE_POINTER.Invoke(cardManager.cardData.id);
            }

            if (cardBeingDragged) cardBeingDragged = false;

            GameManager.Instance.EVENT_TOGGLE_TOOLTIPS.Invoke(true);
        }

        private void OnMouseUpAsButton()
        {
            if (pointerIsActive)
            {
                cardIsShowingUp = false;
                cardIsDisplaced = false;
                cardManager.ResetCardPosition();
                pointerIsActive = false;
            }

            if (cardBeingDragged) cardBeingDragged = false;

            if (cardManager.cardActive)
            {
                if (transform.position.y > GameSettings.HAND_CARD_SHOW_UP_Y &&
                    cardManager.card_can_be_played) //if (overPlayer)
                {
                    Debug.Log("card is on center");
                    // Get Player ID
                    GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.Card, "Play");
                    GameManager.Instance.EVENT_CARD_PLAYED.Invoke(cardManager.cardData.id, "-1");
                    cardManager.cardActive = false;
                }
                else
                {
                    Debug.Log("card is far from center");
                    cardIsDisplaced = true;
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            bool isOver = false;
            if (collision != null && collision.gameObject.CompareTag("Player"))
            {
                var other = collision.gameObject.GetComponentInParent<PlayerManager>();
                if (other != null)
                    isOver = true;
            }

            if (overPlayer != isOver)
            {
                if (isOver)
                {
                    PlayerEnter(collision.gameObject);
                }
                else
                {
                    if (lastOver != null)
                    {
                        PlayerExit(lastOver);
                    }
                }
            }
            else if (isOver && collision.gameObject != lastOver)
            {
                if (lastOver != null)
                {
                    PlayerExit(lastOver);
                }

                PlayerEnter(collision.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            OnTriggerStay2D(null);
        }


        private void OnCardMouseExit(string cardId)
        {
            if (cardId != cardManager.cardData.id && !cardIsShowingUp && !cardBeingDragged)
            {
                // Debug.Log("[OnCardMouseExit]");
                cardIsShowingUp = false;
                cardIsDisplaced = false;
                cardManager.ResetCardPosition();
            }
        }

        private void PlayerEnter(GameObject obj)
        {
            lastOver = obj;
            if (obj.CompareTag("Player") && cardManager.card_can_be_played &&
                transform.position.y > GameSettings.HAND_CARD_SHOW_UP_Y)
            {
                overPlayer = true;
            }
        }

        private void PlayerExit(GameObject obj)
        {
            if (obj.CompareTag("Player"))
            {
                overPlayer = false;
            }

            lastOver = null;
        }
    }
}