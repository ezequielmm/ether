using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardedCardPileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI amountOfCardsTF;
    int cardsDiscarded = 0;
    bool audioRunning = false;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = transform as RectTransform;
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
        GameManager.Instance.EVENT_CARD_DISCARD.AddListener(OnCardDiscard);
    }

    private void OnPilesUpdate(CardPiles data)
    {        
         amountOfCardsTF.SetText(data.data.discard.Count!.ToString());
    }

    private void OnCardDiscard()
    {
        Debug.Log($"[Discard Pile] Card Discarded.");
        cardsDiscarded++;
        StartCoroutine(DiscardCardSFX());
    }

    private IEnumerator DiscardCardSFX()
    {
        if (!audioRunning)
        {
            audioRunning = true;
            for (; cardsDiscarded >= 0; cardsDiscarded--)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Discard");
                yield return new WaitForSeconds(GameSettings.CARD_SFX_MIN_RATE);
                cardsDiscarded = 0; // Forces the audio to only play once
            }
            if (cardsDiscarded < 0)
            {
                cardsDiscarded = 0;
            }
            audioRunning = false;
        }
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        //if (nodeState.data != null && nodeState.data.data != null) amountOfCardsTF.SetText(nodeState.data.data.player.cards!.discard.Count!.ToString());
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Discarded);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(transform.position.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) * 1f),
            transform.position.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) * 0.5f), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.DiscardPileTooltips, TooltipController.Anchor.BottomRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
