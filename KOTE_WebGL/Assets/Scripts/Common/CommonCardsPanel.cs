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
    private Deck discardDeck;
    private Deck exhaustDeck;


    void Start()
    {
        CommonCardsContainer.SetActive(false);
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener(DisplayCards);
        //GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
    }

    private void OnPilesUpdate(CardPiles data)
    {
        drawDeck = new Deck();
        discardDeck = new Deck();
        exhaustDeck = new Deck();

        drawDeck.cards = data.data.draw;
        discardDeck.cards = data.data.discard;
        exhaustDeck.cards = data.data.exhaust;       

    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState,WS_QUERY_TYPE wsType)
    {
        drawDeck = new Deck();
        discardDeck = new Deck();
        exhaustDeck = new Deck();

        if (nodeState != null && nodeState.data !=null && nodeState.data.data != null && nodeState.data.data.player != null && nodeState.data.data.player.cards != null)
        {
            if (nodeState.data.data.player.cards.draw != null)
            {
                drawDeck.cards = nodeState.data.data.player.cards.draw;
            }
            if (nodeState.data.data.player.cards.discard != null)
            {
                discardDeck.cards = nodeState.data.data.player.cards.discard;
            }
            if (nodeState.data.data.player.cards.exhaust != null)
            {
                exhaustDeck.cards = nodeState.data.data.player.cards.exhaust;
            }
        }
        
    }

    private void OnPlayerStatusUpdate(PlayerStateData playerState)
    {
        playerDeck.cards = playerState.data.playerState.cards;
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
            case PileTypes.Discarded: GenerateCards(discardDeck); break;
            case PileTypes.Exhausted: GenerateCards(exhaustDeck); break;
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
