using System.Collections;
using System.Collections.Generic;
using KOTE.Expedition.Combat.Cards.Piles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class HandManagerTests : MonoBehaviour
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

    private GameObject hand;
    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject HandPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Hand.prefab"); 
        hand = Instantiate(HandPilePrefab);
        hand.SetActive(true);
        EventSystem eventSystem = hand.AddComponent<EventSystem>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(hand);
        GameManager.Instance.DestroyInstance();
        yield return null;
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
        Assert.AreEqual("Draw Single", sfxType);
    }
}