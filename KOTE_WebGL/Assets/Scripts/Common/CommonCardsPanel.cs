using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCardsPanel : MonoBehaviour
{
    public GameObject CommonCardsContainer;
    public GameObject gridCardsContainer;
    public GameObject cardPrefab;

    private Deck playerDeck;
    private Deck drawDeck;


    void Start()
    {
        CommonCardsContainer.SetActive(false);
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener(DisplayCards);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState)
    {
        drawDeck = new Deck();
        drawDeck.cards = nodeState.data.data.player.cards.draw;
    }

    private void OnPlayerStatusUpdate(PlayerStateData playerState)
    {
        playerDeck = playerState.data.player_state.deck;
    }

    private void DisplayCards(PileTypes pileType)
    {
        if (CommonCardsContainer.activeSelf)
        {
            CommonCardsContainer.SetActive(false);
        }
        else
        {
            CommonCardsContainer.SetActive(true);
            ShowCards(pileType);
        }
        
        
    }

    private void ShowCards(PileTypes pileType)
    {
        for (int i =0; i < gridCardsContainer.transform.childCount;i++)
        {
            Destroy(gridCardsContainer.transform.GetChild(i).gameObject);
        }

        switch (pileType)
        {
            case PileTypes.Deck: GenerateCards(playerDeck); break;
            case PileTypes.Draw: GenerateCards(drawDeck); break;
        }       
    }

    private void GenerateCards(Deck deck)
    {
        if (deck != null && deck.cards !=null)
        {
            foreach (Card card in deck.cards)
            {
                GameObject newCard = Instantiate(cardPrefab, gridCardsContainer.transform);
                newCard.GetComponent<UICardPrefabManager>().populate(card);
            }
        }
    }

}
