using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KOTE.Expedition.Combat.Cards.Piles
{
    public class ExhaustPileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        RectTransform rectTransform;
        public TextMeshProUGUI amountOfCardsTF;
        public List<CardManager> exhaustDeck = new();

        void Start()
        {
            rectTransform = transform as RectTransform;
            GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
            GameManager.Instance.EVENT_CARD_EXHAUST.AddListener(OnCardExhausted);
        }

        private void OnPilesUpdate(CardPiles data)
        {
            amountOfCardsTF.SetText(data.data.exhausted.Count!.ToString());
        }

        private void OnCardExhausted()
        {
            Debug.Log($"[Exhaust Pile] Card Exhausted.");
        }


        public void OnPileClick()
        {
            GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Exhausted);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Vector3 anchorPoint = new Vector3(
                transform.position.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) * 1f),
                transform.position.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) * 0.5f), 0);
            anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
            // Tooltip On
            GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.ExhaustPileTooltips,
                TooltipController.Anchor.BottomRight, anchorPoint, null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Tooltip Off
            GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        }
    }
}