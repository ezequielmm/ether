using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonCardsPanel : CardPanelBase
{
    public GameObject uiCardPrefab;

    private Deck playerDeck;
    private Deck drawDeck;
    private Deck discardDeck;
    private Deck exhaustDeck;


    protected override void Start()
    {
        base.Start();
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener(DisplayCards);
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener(OnPilesUpdate);
        GameManager.Instance.EVENT_CARD_PILE_SHOW_DECK.AddListener(onFullDeckShow);
    }

    private void OnPilesUpdate(CardPiles data)
    {
        drawDeck = new Deck();
        discardDeck = new Deck();
        exhaustDeck = new Deck();

        drawDeck.cards = data.data.draw;
        discardDeck.cards = data.data.discard;
        exhaustDeck.cards = data.data.exhausted;
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
            if (nodeState.data.data.player.cards.exhausted != null)
            {
                exhaustDeck.cards = nodeState.data.data.player.cards.exhausted;
            }
        }
        
    }

    private void DisplayCards(PileTypes pileType)
    {
        if (commonCardsContainer.activeSelf)
        {
            commonCardsContainer.SetActive(false);
        }
        else
        {
            commonCardsContainer.SetActive(true);
            ShowCards(pileType);
        }
        
        
    }

    private void onFullDeckShow(Deck deck) 
    {
        playerDeck = deck;
        commonCardsContainer.SetActive(true);
        DestroyCards();
        GenerateCards(playerDeck);
    }
    
    private void ShowCards(PileTypes pileType)
    {
        DestroyCards();
        switch (pileType)
        {
            case PileTypes.Deck: GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.PlayerDeck); break;
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
                GameObject newCard = Instantiate(uiCardPrefab, gridCardsContainer.transform);
                newCard.GetComponent<UICardPrefabManager>().populate(card);
            }
        }
    }

}
