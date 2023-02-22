using System.Collections;
using System.Collections.Generic;
using CardManagement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class HandManagerTests
{
    private HandManager _handManager;

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

    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/Expedition");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(true);
        GameObject hand = GameObject.Find("Hand");
        _handManager = hand.GetComponent<HandManager>();
    }

    [UnityTest]
    public IEnumerator DoesStartFireGenericDataEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) => { eventFired = true; });
        GameObject go = new GameObject();
        HandManager handManager = go.AddComponent<HandManager>();
        go.SetActive(true);
        handManager.enabled = true;
        yield return null;
        Assert.True(eventFired);
    }

    [Test]
    public void DoesDrawingACardFireSFXEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
        GameManager.Instance.EVENT_CARD_DRAW.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesDrawingACardFireCorrectSFX()
    {
        string sfxType = "";
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { sfxType = data2; });
        GameManager.Instance.EVENT_CARD_DRAW.Invoke();
        Assert.AreEqual("Card Draw", sfxType);
    }

    [Test]
    public void DoesDrawingCardsWithoutDataLogMessage()
    {
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        LogAssert.Expect(LogType.Log, "[HandManager] No cards data at all. Retrieving");
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
        LogAssert.Expect(LogType.Log, "[HandManager] No hands cards data. Retrieving");
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
            $"[HandManager] draw.count: {drawDeck.cards.Count} | hand.count: {handDeck.cards.Count} | discard.count: {discardDeck.cards.Count} | exhaust.count: {exhaustDeck.cards.Count}");
    }
}