using System.Collections;
using System.Collections.Generic;
using CardManagement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class DrawPileManagerTests : MonoBehaviour
{
    private DrawPileManager _drawManager;
    private GameObject go;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // add a camera so that things will run
        go = new GameObject();
        Camera camera = go.AddComponent<Camera>();
        camera.tag = "MainCamera";

        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DrawCardPile.prefab");
        GameObject drawPileManager = Instantiate(drawPilePrefab);
        _drawManager = drawPileManager.GetComponent<DrawPileManager>();
        drawPileManager.SetActive(true);
        EventSystem eventSystem = drawPileManager.AddComponent<EventSystem>();


        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_drawManager.gameObject);
        Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesShufflingMoreThanOneCardFireSFXEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
        GameManager.Instance.EVENT_CARD_SHUFFLE.Invoke();
        yield return new WaitForSeconds(0.3f);
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesShufflingMoreThanOneCardFireCorrectSFX()
    {
        SoundTypes soundType = SoundTypes.EnemyOffensive;
        string sfxType = "";
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) =>
        {
            soundType = data; sfxType = data2; });
        GameManager.Instance.EVENT_CARD_SHUFFLE.Invoke();
        yield return new WaitForSeconds(0.3f);
        Assert.AreEqual(SoundTypes.Card, soundType);
        Assert.AreEqual("Shuffle", sfxType);
    }

    [Test]
    public void DoesUpdatingPilesUpdatePileAmount()
    {
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(new CardPiles
        {
            data = new Cards
            {
                draw = new List<Card> { new Card() }
            }
        });
        Assert.AreEqual("1", _drawManager.amountOfCardsTF.text);
    }

    [Test]
    public void DoesClickingThePileFirePileClickedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener((data) => { eventFired = true; });
        _drawManager.OnPileClick();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnPointerEnterFireSetToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener((data, data2, data3, data4) => { eventFired = true; });
        _drawManager.OnPointerEnter(new PointerEventData(EventSystem.current));
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnPointerExitFireClearToolTipEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener(() => { eventFired = true; });
        _drawManager.OnPointerExit(null);
        Assert.True(eventFired);
    }
}