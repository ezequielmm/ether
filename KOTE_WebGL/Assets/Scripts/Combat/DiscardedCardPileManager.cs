using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiscardedCardPileManager : MonoBehaviour
{
    public TextMeshProUGUI amountOfCardsTF;
    void Start()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, bool initialCall)
    {
        amountOfCardsTF.SetText(nodeState.data.data.player.cards!.discard.Count!.ToString());
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Discarded);
    }
}
