using System.Collections;
using System.Collections.Generic;
using KOTE.Expedition.Combat.Cards.Piles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class CardPilesManagerTests : MonoBehaviour
{
    private Deck drawDeck = new Deck
    {
        cards = new List<Card>
        {
            new Card
            {
                id = "drawTest", cardId = 1, cardType = "attack", pool = "knight", description = "", energy = 1,
                isUpgraded = false, keywords = new List<string>(), name = "test", properties = new Effects(),
                rarity = "rare", showPointer = false
            }
        }
    };

    private Deck handDeck = new Deck
    {
        cards = new List<Card>
        {
            new Card
            {
                id = "drawTest", cardId = 1, cardType = "attack", pool = "knight", description = "", energy = 1,
                isUpgraded = false, keywords = new List<string>(), name = "test", properties = new Effects(),
                rarity = "rare", showPointer = false
            }
        }
    };

    private Deck discardDeck = new Deck
    {
        cards = new List<Card>
        {
            new Card
            {
                id = "discardTest", cardId = 1, cardType = "attack", pool = "knight", description = "", energy = 1,
                isUpgraded = false, keywords = new List<string>(), name = "test", properties = new Effects(),
                rarity = "rare", showPointer = false
            }
        }
    };

    private Deck exhaustDeck = new Deck
    {
        cards = new List<Card>
        {
            new Card
            {
                id = "exhaustTest", cardId = 1, cardType = "attack", pool = "knight", description = "", energy = 1,
                isUpgraded = false, keywords = new List<string>(), name = "test", properties = new Effects(),
                rarity = "rare", showPointer = false
            }
        }
    };

    private GameObject cardPilesManager;
    private GameObject exhaustPileManager;
    private GameObject drawPileManager;
    private GameObject discardPileManager;
    private GameObject handManager;
    private GameObject spriteManager;
    private GameObject go;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // add a camera so that things will run
        GameObject go = new GameObject();
        Camera camera = go.AddComponent<Camera>();
        camera.tag = "MainCamera";

        GameObject HandPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Hand.prefab");
        handManager = Instantiate(HandPilePrefab);
        HandManager _handManager = handManager.GetComponent<HandManager>();

        GameObject exhaustedCardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/ExhaustedPilePrefab.prefab");
        exhaustPileManager = Instantiate(exhaustedCardPilePrefab);
        exhaustPileManager.name = "ExhaustedPilePrefab";
        ExhaustPileManager _exhaustManager = exhaustPileManager.GetComponent<ExhaustPileManager>();

        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DrawCardPile.prefab");
        drawPileManager = Instantiate(drawPilePrefab);
        drawPileManager.name = "DrawCardPile";
        DrawPileManager _drawManager = drawPileManager.GetComponent<DrawPileManager>();
        drawPileManager.SetActive(true);

        GameObject DiscardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DiscardCardPile.prefab");
        discardPileManager = Instantiate(DiscardPilePrefab);
        discardPileManager.name = "DiscardCardPile";
        DiscardPileManager _discardManager = discardPileManager.GetComponent<DiscardPileManager>();
        discardPileManager.SetActive(true);

        GameObject CardPilesManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/CardPilesManager.prefab");
        cardPilesManager = Instantiate(CardPilesManagerPrefab);
        cardPilesManager.SetActive(true);
        CardPilesManager cardPiles = cardPilesManager.GetComponent<CardPilesManager>();

        cardPiles.discardManager = _discardManager;
        cardPiles.drawManager = _drawManager;
        cardPiles.exhaustManager = _exhaustManager;
        cardPiles.handManager = _handManager;

        GameObject SpriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(SpriteManagerPrefab);
        drawPileManager.SetActive(true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(cardPilesManager);
        Destroy(exhaustPileManager);
        Destroy(drawPileManager);
        Destroy(discardPileManager);
        Destroy(handManager);
        Destroy(spriteManager);
        Destroy(go);
        GameManager.Instance.DestroyInstance();
        yield return null;
    }

    [Test]
    public void DoesDrawingCardsWithoutDataLogMessage()
    {
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        LogAssert.Expect(LogType.Log,
            $"[CardPilesManager] Insufficient card data to draw cards. Hand Count is  Requesting Card Piles");
    }

    [Test]
    public void DoesDrawingCardsWithoutHandDataLogMessage()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                hand = new List<Card>(),
                exhausted = new List<Card>(),
                discard = new List<Card>(),
                draw = new List<Card>()
            }
        });
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        LogAssert.Expect(LogType.Log,
            $"[CardPilesManager] Insufficient card data to draw cards. Hand Count is 0 Requesting Card Piles");
    }

    [Test]
    public void DoesDrawingCardsWithProperDataLogMessage()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                hand = handDeck.cards,
                discard = discardDeck.cards,
                draw = drawDeck.cards,
                exhausted = exhaustDeck.cards
            }
        });
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        LogAssert.Expect(LogType.Log,
            $"[CardPilesManager] Card Piles Retrieved. draw.count: 1 | hand.count: 1 | discard.count: 1 | exhaust.count: 1");
    }

    [Test]
    public void DoesDrawingCardsWithoutDataMakeGenericDataRequest()
    {
        bool eventFired = false;
        WS_DATA_REQUEST_TYPES requestType = WS_DATA_REQUEST_TYPES.Potions;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((dataType) =>
        {
            eventFired = true;
            requestType = dataType;
        });
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        Assert.True(eventFired);
        Assert.AreEqual(WS_DATA_REQUEST_TYPES.CardsPiles, requestType);
    }

    [Test]
    public void DoesDrawingCardsWithoutHandDataMakeGenericDataRequest()
    {
        bool eventFired = false;
        WS_DATA_REQUEST_TYPES requestType = WS_DATA_REQUEST_TYPES.Potions;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((dataType) =>
        {
            eventFired = true;
            requestType = dataType;
        });
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                hand = new List<Card>(),
                exhausted = new List<Card>(),
                discard = new List<Card>(),
                draw = new List<Card>()
            }
        });
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        Assert.True(eventFired);
        Assert.AreEqual(WS_DATA_REQUEST_TYPES.CardsPiles, requestType);
    }
}