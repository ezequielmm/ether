using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class DrawCardPileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI amountOfCardsTF;
    int cardsShuffled = 0;
    bool audioRunning = false;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = transform as RectTransform;
        // GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
        GameManager.Instance.EVENT_CARD_SHUFFLE.AddListener(OnShuffle);
    }

    private void OnShuffle()
    {
        Debug.Log($"[Draw Pile] Card Shuffled.");
        cardsShuffled++;
        StartCoroutine(ShuffleCardSFX());
    }

    private IEnumerator ShuffleCardSFX()
    {
        if (!audioRunning)
        {
            audioRunning = true;
            for (; cardsShuffled >= 0; cardsShuffled--)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Deck Shuffle");
                yield return new WaitForSeconds(GameSettings.CARD_SFX_MIN_RATE);
                cardsShuffled = 0; // Forces this to only run once
            }
            if (cardsShuffled < 0)
            {
                cardsShuffled = 0;
            }
            audioRunning = false;
        }
    }

    private void OnPilesUpdate(CardPiles data)
    {
        amountOfCardsTF.SetText(data.data.draw.Count!.ToString());
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
       // if (nodeState.data != null && nodeState.data.data != null) amountOfCardsTF.SetText(nodeState.data.data.player.cards.draw.Count!.ToString());
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Draw);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(transform.position.x + ((rectTransform.rect.width * rectTransform.lossyScale.x) * 1f),
            transform.position.y, 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.DrawPileTooltips, TooltipController.Anchor.BottomLeft, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
