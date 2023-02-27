using System.Collections;
using System.Collections.Generic;
using KOTE.Expedition.Combat.Cards.Piles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class ExhaustPileManagerTests : MonoBehaviour
{
    private ExhaustPileManager _exhaustManager;
    private GameObject go;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // add a camera so that things will run
        go = new GameObject();
        Camera camera = go.AddComponent<Camera>();
        camera.tag = "MainCamera";

        GameObject exhaustedCardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/ExhaustedPilePrefab.prefab");
        GameObject exhaustPileManager = Instantiate(exhaustedCardPilePrefab);
        _exhaustManager = exhaustPileManager.GetComponent<ExhaustPileManager>();
        exhaustPileManager.SetActive(true);
        EventSystem eventSystem = exhaustPileManager.AddComponent<EventSystem>();


        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_exhaustManager.gameObject);
        Destroy(go);
        GameManager.Instance.DestroyInstance();

        yield return null;
    }

    [Test]
    public void DoesExhaustingaCardFireSFXEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
        GameManager.Instance.EVENT_CARD_EXHAUST.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesExhaustingaCardFireCorrectSFX()
    {
        string sfxType = "";
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { sfxType = data2; });
        GameManager.Instance.EVENT_CARD_EXHAUST.Invoke();
        Assert.AreEqual("Exhaust", sfxType);
    }

    [Test]
    public void DoesExhaustingCardLogThatCardWasDiscarded()
    {
        GameManager.Instance.EVENT_CARD_EXHAUST.Invoke();
        LogAssert.Expect(LogType.Log, "[Exhaust Pile] Card Exhausted.");
    }

    [Test]
    public void DoesUpdatingPilesUpdatePileAmount()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                exhausted = new List<Card> { new Card() }
            }
        });
        Assert.AreEqual("1", _exhaustManager.amountOfCardsTF.text);
    }

    [Test]
    public void DoesClickingThePileFirePileClickedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener((data) => { eventFired = true; });
        _exhaustManager.OnPileClick();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnPointerEnterFireSetToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener((data, data2, data3, data4) => { eventFired = true; });
        _exhaustManager.OnPointerEnter(new PointerEventData(EventSystem.current));
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnPointerExitFireClearToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener(() => { eventFired = true; });
        _exhaustManager.OnPointerExit(null);
        Assert.True(eventFired);
    }
}