using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExhaustedCardPileManager : MonoBehaviour
{
    public TextMeshProUGUI amountOfCardsTF;
    void Start()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        if (nodeState.data != null && nodeState.data.data != null) amountOfCardsTF.SetText(nodeState.data.data.player.cards!.exhaust.Count!.ToString());
    }

    public void OnPileClick()
    {
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.Invoke(PileTypes.Discarded);
    }
}
