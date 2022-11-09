using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DiscardedCardPileManagerTests : MonoBehaviour
{
    private DiscardedCardPileManager _discardManager;
    private GameObject cameraObject;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // add a camera so that things will run
         cameraObject = new GameObject();
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        
        GameObject DiscardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DiscardCardPile.prefab");
        GameObject discardPile = GameObject.Instantiate(DiscardPilePrefab);
        _discardManager = discardPile.GetComponent<DiscardedCardPileManager>();
        discardPile.SetActive(true);
        EventSystem eventSystem = discardPile.AddComponent<EventSystem>();
        

        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_discardManager.gameObject);
        Destroy(cameraObject);
        yield return null;
    }

    [Test]
    public void DoesAmountOfCardsUpdateWhenCardPilesAreSent()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                discard = new List<Card>
                {
                    new Card()
                }
            }
        });
        Assert.AreEqual("1", _discardManager.amountOfCardsTF.text);

        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                discard = new List<Card>()
            }
        });
        Assert.AreEqual("0", _discardManager.amountOfCardsTF.text);
    }

    [Test]
    public void DoesDiscardingCardLogThatCardWasDiscarded()
    {
        GameManager.Instance.EVENT_CARD_DISCARD.Invoke();
        LogAssert.Expect(LogType.Log, "[Discard Pile] Card Discarded.");
    }

    [Test]
    public void DoesDiscardingCardFirePlaySfxEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_CARD_DISCARD.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesDiscardingCardEventPlayCorrectSound()
    {
        string eventContent = "";
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data) => { eventContent = data; });
        GameManager.Instance.EVENT_CARD_DISCARD.Invoke();
        Assert.AreEqual("Card Discard", eventContent);
    }

    [Test]
    public void DoesOnPileClickFireCardPileClickedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener((data) => { eventFired = true; });
        _discardManager.OnPileClick();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnPileClickFireEventWithCorrectPileType()
    {
        PileTypes pileType = PileTypes.Deck;
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener((data) => { pileType = data; });
        _discardManager.OnPileClick();
        Assert.AreEqual(PileTypes.Discarded, pileType);
    }

    [Test]
    public void DoesOnPointerEnterFireSetToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener((data, data2, data3, data4) => { eventFired = true; });
        _discardManager.OnPointerEnter(new PointerEventData(EventSystem.current));
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesOnPointerExitFireClearToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener(() => { eventFired = true; });
        _discardManager.OnPointerExit(null);
        Assert.True(eventFired);
    }
}