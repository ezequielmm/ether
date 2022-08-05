using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ExhaustedCardPileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    public TextMeshProUGUI amountOfCardsTF;
    int cardsExhausted = 0;
    bool audioRunning = false;
    void Start()
    {
        rectTransform = transform as RectTransform;
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
        GameManager.Instance.EVENT_CARD_EXHAUST.AddListener(OnCardExhausted);
    }

    private void OnPilesUpdate(CardPiles data)
    {
        amountOfCardsTF.SetText(data.data.exhaust.Count!.ToString());
    }

    private void OnCardExhausted()
    {
        Debug.Log($"[Exhaust Pile] Card Exhausted.");
        cardsExhausted++;
        StartCoroutine(ExhaustedCardSFX());
    }

    private IEnumerator ExhaustedCardSFX()
    {
        if (!audioRunning)
        {
            audioRunning = true;
            for (; cardsExhausted >= 0; cardsExhausted--)
            {
                GameManager.Instance.EVENT_PLAY_SFX.Invoke("Card Exhaust");
                yield return new WaitForSeconds(GameSettings.CARD_SFX_MIN_RATE);
            }
            if (cardsExhausted < 0)
            {
                cardsExhausted = 0;
            }
            audioRunning = false;
        }
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        if (nodeState.data != null && nodeState.data.data != null) amountOfCardsTF.SetText(nodeState.data.data.player.cards!.exhaust.Count!.ToString());
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Discarded);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(transform.position.x + rectTransform.rect.center.x - ((rectTransform.rect.width * rectTransform.lossyScale.x) / 2),
            transform.position.y + rectTransform.rect.center.y - ((rectTransform.rect.height * rectTransform.lossyScale.y) / 2), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        List<Tooltip> tooltips = new List<Tooltip>() { new Tooltip()
        {
            title = "Exhausted Cards",
            description = "Click to view cards Exhausted this combat."
        }};
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(tooltips, TooltipController.Anchor.BottomRight, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}
