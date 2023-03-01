using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KOTE.Expedition.Combat.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class CardManagerTests : MonoBehaviour
{
    private CardManager cardManager;
    private CardVisualsManager _visualsManager;
    private GameObject drawPileManager;
    private GameObject discardPile;
    private GameObject exhaustPileManager;
    private GameObject spriteManager;
    private GameObject cameraGo;
    private GameObject spriteCardInstance;

    private Transform drawPos;
    private Transform exhaustPos;
    private Transform discardPos;

    private Vector3[] pileOrthoPos = new[] { Vector3.zero, Vector3.zero, Vector3.zero };

    private Card testCard;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        testCard = new Card
        {
            cardId = 1,
            cardType = "attack",
            description = "description",
            energy = 3,
            id = "test",
            isUpgraded = false,
            keywords = new List<string>(),
            name = "Test Card",
            pool = "knight",
            properties = new Effects
            {
                effects = new List<Effect>(),
                statuses = new List<Statuses>
                {
                    new Statuses
                    {
                        args = new Statuses.Args
                        {
                            attachTo = "player",
                            description = "description",
                            value = 1
                        },
                        name = "status",
                        tooltip = new Tooltip
                        {
                            description = "Tooltip Description",
                            title = "ToolTip title"
                        }
                    },
                    new Statuses
                    {
                        args = new Statuses.Args
                        {
                            attachTo = "player",
                            description = "description",
                            value = 1
                        },
                        name = "status2",
                        tooltip = new Tooltip()
                    }
                }
            },
            showPointer = false,
            rarity = "common"
        };

        // add a camera so that things will run
        cameraGo = new GameObject();
        Camera newCamera = cameraGo.AddComponent<Camera>();
        newCamera.tag = "MainCamera";

        GameObject drawPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DrawCardPile.prefab");
        drawPileManager = Instantiate(drawPilePrefab, new Vector3(10, 10, 0), Quaternion.identity);
        drawPileManager.name = "DrawCardPile";
        drawPos = drawPileManager.transform;
        drawPileManager.SetActive(true);
        EventSystem eventSystem = drawPileManager.AddComponent<EventSystem>();

        GameObject DiscardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/DiscardCardPile.prefab");
        discardPile = GameObject.Instantiate(DiscardPilePrefab, new Vector3(100, 100, 0), Quaternion.identity);
        discardPos = discardPile.transform;
        discardPile.name = "DiscardCardPile";
        discardPile.SetActive(true);


        GameObject exhaustedCardPilePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/ExhaustedPilePrefab.prefab");
        exhaustPileManager = Instantiate(exhaustedCardPilePrefab, new Vector3(50, 50, 0), Quaternion.identity);
        exhaustPos = exhaustPileManager.transform;
        exhaustPileManager.name = "ExhaustedPilePrefab";
        exhaustPileManager.SetActive(true);

        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        GameObject spriteCardPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/SpriteCardPrefab.prefab");
        spriteCardInstance = Instantiate(spriteCardPrefab);
        cardManager = spriteCardInstance.GetComponent<CardManager>();
        _visualsManager = spriteCardInstance.GetComponent<CardVisualsManager>();
        cardManager.Populate(new Card(), 0, pileOrthoPos);
        spriteCardInstance.SetActive(true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(cardManager.gameObject);
        Destroy(discardPile);
        Destroy(exhaustPileManager);
        Destroy(cameraGo);
        Destroy(spriteManager);
        Destroy(spriteCardInstance);
        Destroy(drawPileManager);
        GameManager.Instance.DestroyInstance();
        yield return null;
    }

    [Test]
    public void DoesUpdatingCardActiveStateUpdateCardBasedOnCurrentEnergy()
    {
        //TODO write this once I understand how this works
    }

    [Test]
    public void DoesTooltipListGetCreatedDuringAwake()
    {
        Assert.NotNull(_visualsManager.tooltips);
        Assert.IsInstanceOf<List<Tooltip>>(_visualsManager.tooltips);
        Assert.AreEqual(0, _visualsManager.tooltips.Count);
    }

    [Test]
    public void DoesDestroyOnGameStatusComponentGetAddedOnStart()
    {
        Assert.NotNull(cardManager.gameObject.GetComponent<DestroyOnGameStatus>());
    }

    [Test]
    public void DoesDestroyOnGameStatusComponentGetPopulatedWithCorrectDeathCause()
    {
        DestroyOnGameStatus deathStatus = cardManager.gameObject.GetComponent<DestroyOnGameStatus>();
        Assert.AreEqual(1, deathStatus.causesOfDeath.Count);
        Assert.AreEqual(true, deathStatus.causesOfDeath[0].UnParent);
        Assert.AreEqual(GameStatuses.GameOver, deathStatus.causesOfDeath[0].StatusToListenTo);
        Assert.AreEqual(1f, deathStatus.causesOfDeath[0].AnimationTime);
        Assert.AreEqual(true, deathStatus.causesOfDeath[0].ShrinkToDie);
    }

    [Test]
    public void DoesPopulateUpdateTextFields()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(testCard.energy.ToString(), _visualsManager.energyTF.text);
        Assert.AreEqual(testCard.rarity, _visualsManager.rarityTF.text);
        Assert.AreEqual(testCard.name, _visualsManager.nameTF.text);
        Assert.AreEqual(testCard.description, _visualsManager.descriptionTF.text);
    }

    [Test]
    public void DoesPopulateUpdateEnergyAmount()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(testCard.energy.ToString(), _visualsManager.energyTF.text);
    }

    [Test]
    public void DoesPopulateChangeEnergyToXIfLessThanZero()
    {
        testCard.energy = -1;
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual("X", _visualsManager.energyTF.text);
    }

    [Test]
    public void DoesUpdateCardTextUpdateDescriptionField()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        testCard.description = "new description";
        GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(testCard);
        Assert.AreEqual(testCard.description, _visualsManager.descriptionTF.text);
    }

    [Test]
    public void DoesUpdateCardTextUpdateNameField()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        testCard.name = "Brace";
        GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(testCard);
        Assert.AreEqual(testCard.name, _visualsManager.nameTF.text);
    }

    [Test]
    public void DoesUpdateCardTextUpdateRarityField()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        testCard.rarity = "TestRarity";
        GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(testCard);
        Assert.AreEqual(testCard.rarity, _visualsManager.rarityTF.text);
    }

    [Test]
    public void DoesUpdateCardChangeEnergyToXIfLessThanZero()
    {
        testCard.energy = 1;
        cardManager.Populate(testCard, 1, pileOrthoPos);
        testCard.energy = -1;
        GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(testCard);
        Assert.AreEqual("X", _visualsManager.energyTF.text);
    }

    [Test]
    public void DoesUpdateCardTextUpdateEnergyField()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        testCard.energy = 0;
        GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(testCard);
        Assert.AreEqual(testCard.energy.ToString(), _visualsManager.energyTF.text);
    }

    [Test]
    public void DoesPopulateSelectCorrectSprites()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual("KOTE_Asset_Gem_Attack", _visualsManager.gemSprite.sprite.name);
        Assert.AreEqual("KOTE_Asset_Frame_Knight", _visualsManager.frameSprite.sprite.name);
        Assert.AreEqual("Green", _visualsManager.bannerSprite.sprite.name);
        Assert.AreEqual("1", _visualsManager.cardImage.sprite.name);
    }

    [Test]
    public void DoesPopulateSaveCardData()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(testCard.id, cardManager.id);
    }

    [Test]
    public void DoesPopulateGenerateTooltips()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(2, _visualsManager.tooltips.Count);
        Assert.AreEqual("ToolTip title", _visualsManager.tooltips[0].title);
        Assert.AreEqual("Tooltip Description", _visualsManager.tooltips[0].description);
        Assert.AreEqual("Status2", _visualsManager.tooltips[1].title);
        Assert.AreEqual("description", _visualsManager.tooltips[1].description);
    }

    [Test]
    public void DoesPopulateUpdateCardBasedOnEnergy()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(_visualsManager.redColor, _visualsManager.energyTF.color);
    }

    [Test]
    public void DoesMoveFromDrawShrinkLocalScale()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, cardManager.transform.localScale.z);
    }

    [Test]
    public void DoesMoveFromDiscardShrinkLocalScale()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.discard, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.z);
    }

    [Test]
    public void DoesMoveFromHandShrinkLocalScale()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.hand, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1, cardManager.transform.localScale.z);
    }

    [Test]
    public void DoesMoveFromExhaustShrinkLocalScale()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.z);
    }

    [Test]
    public void DoesMoveToDrawFireShuffleEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_SHUFFLE.AddListener(() => eventFired = true);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.draw);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesMoveToDiscardFireDiscardEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DISCARD.AddListener(() => eventFired = true);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.discard);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesMoveToHandFireDrawEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DRAW.AddListener(() => eventFired = true);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.hand);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesMoveToExhaustFireExhaustEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_EXHAUST.AddListener(() => eventFired = true);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.exhaust);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesMovingCardToHandActivateCardContent()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand);
        Assert.True(_visualsManager.cardcontent.activeSelf);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardWithDelayWaitBeforeMoving()
    {
        Vector3 originPos = TransformUIToOrtho("DrawCardPile");
        cardManager.Populate(testCard, 0, new[] { originPos, originPos, originPos });
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.exhaust, moveDelay: 0.5f).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(originPos.x, cardManager.transform.position.x, 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(originPos.y, cardManager.transform.position.y, 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(originPos.z, cardManager.transform.position.z, 0.01f);
        yield return new WaitForSeconds(0.6f);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(originPos.x, cardManager.transform.position.x, 0.01f);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(originPos.y, cardManager.transform.position.y, 0.01f);
        Assert.Greater(originPos.z, cardManager.transform.position.z);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardWithDelayMoveCardToDestination()
    {
        Vector3 FinalPos = TransformUIToOrtho("DiscardCardPile");
        cardManager.Populate(testCard, 0, new[] { new Vector3(0, 0, FinalPos.z), FinalPos, Vector3.zero });
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.discard, moveDelay: 0.5f).Play();
        yield return new WaitForSeconds(0.501f);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(FinalPos.x, cardManager.transform.position.x, 0.01f);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(FinalPos.y, cardManager.transform.position.y, 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.z, cardManager.transform.position.z, 0.01f);
        yield return new WaitForSeconds(1.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.x, cardManager.transform.position.x, 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.y, cardManager.transform.position.y, 0.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.z, cardManager.transform.position.z, 0.01f);
    }

    [Test]
    public void DoesMovingCardWithDelayFromDrawToHandSetLocalScaleToZero()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, moveDelay: 0.5f).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardWithDelayFromDrawToHandScaleUpFromZero()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, moveDelay: 0.5f).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
        yield return new WaitForSeconds(1.6f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.one.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.one.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.one.z, cardManager.transform.localScale.z);
    }


    [UnityTest]
    public IEnumerator DoesMovingCardWithDelayScaleDownToZero()
    {
        Vector3 currentScale = cardManager.transform.localScale;
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.exhaust, moveDelay: 0.5f).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.z);
        yield return new WaitForSeconds(1.6f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
    }


    [UnityTest]
    public IEnumerator DoesMovingCardMoveCardToDestination()
    {
        Vector3 FinalPos = TransformUIToOrtho("DiscardCardPile");
        cardManager.Populate(testCard, 0, new[] { new Vector3(0, 0, FinalPos.z), FinalPos, Vector3.zero });
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(FinalPos.x, cardManager.transform.position.x);
        UnityEngine.Assertions.Assert.AreNotApproximatelyEqual(FinalPos.y, cardManager.transform.position.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.z, cardManager.transform.position.z);
        yield return new WaitForSeconds(1.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.x, cardManager.transform.position.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.y, cardManager.transform.position.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(FinalPos.z, cardManager.transform.position.z);
    }

    [Test]
    public void DoesMovingCardFromDrawToHandSetLocalScaleToZero()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardFromDrawToHandRotateToZero()
    {
        cardManager.transform.eulerAngles = Vector3.one;
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand).Play();
        yield return new WaitForSeconds(1f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.rotation.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.rotation.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.rotation.z);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardFromDrawToHandScaleUpFromZero()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
        yield return new WaitForSeconds(1.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.5f, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.5f, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(1.5f, cardManager.transform.localScale.z);
    }


    [UnityTest]
    public IEnumerator DoesMovingCardScaleDownToZero()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.discard).Play();
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0.2f, cardManager.transform.localScale.z);
        yield return new WaitForSeconds(1.01f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.x, cardManager.transform.localScale.x);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.y, cardManager.transform.localScale.y);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(Vector3.zero.z, cardManager.transform.localScale.z);
    }

    [UnityTest]
    public IEnumerator DoesOnMoveCompletedStopMoveParticleSystem()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.hand, CARDS_POSITIONS_TYPES.discard).Play();
        yield return new WaitForSeconds(1.01f);
        Assert.False(_visualsManager.movePs.isPlaying);
        Assert.True(_visualsManager.movePs.isStopped);
    }

    [UnityTest]
    public IEnumerator DoesOnMoveCompleteFireDisableCardEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DISABLED.AddListener((data) => eventFired = true);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.hand, CARDS_POSITIONS_TYPES.discard).Play();
        yield return new WaitForSeconds(1.01f);
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesOnMoveCompletedUpdateCardBasedOnEnergy()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(_visualsManager.redColor, _visualsManager.energyTF.color);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(3, 3);
        yield return new WaitForSeconds(1.01f);
        Assert.AreEqual(Color.black, _visualsManager.energyTF.color);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardWithDelayFromDrawToHandProcessOnMoveCompleted()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(_visualsManager.redColor, _visualsManager.energyTF.color);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand, moveDelay: 0.5f);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(3, 3);
        yield return new WaitForSeconds(1.6f);
        Assert.AreEqual(Color.black, _visualsManager.energyTF.color);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardFromDrawToHandProcessOnMoveCompleted()
    {
        cardManager.Populate(testCard, 1, pileOrthoPos);
        Assert.AreEqual(_visualsManager.redColor, _visualsManager.energyTF.color);
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.draw, CARDS_POSITIONS_TYPES.hand);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(3, 3);
        yield return new WaitForSeconds(1.01f);
        Assert.AreEqual(Color.black, _visualsManager.energyTF.color);
    }

    [UnityTest]
    public IEnumerator DoesMovingCardWithDelaProcessHideAndDeactivateCard()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.hand, moveDelay: 0.5f);
        //TODO return to this once HideAndDeactivateCard tests are written
        yield break;
    }

    [UnityTest]
    public IEnumerator DoesMovingCardProcessHideAndDeactivateCard()
    {
        cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.hand);
        //TODO return to this once HideAndDeactivateCard tests are written
        yield break;
    }

    [Test]
    public void DoesMoveCardReturnSequence()
    {
        Sequence sequence = cardManager.MoveCard(CARDS_POSITIONS_TYPES.exhaust, CARDS_POSITIONS_TYPES.hand);
        Assert.NotNull(sequence);
    }
    
    private Vector3 TransformUIToOrtho(string uiName)
    {
        Vector3 pos = GameObject.Find(uiName).transform.position; //(1.1, 104.5, 0.0)

        float height = 2 * Camera.main.orthographicSize; //10
        float width = height * Camera.main.aspect; //21.42

        //transform UI coordinates to orthorgraphic coordinates
        float xx = pos.x * width / Screen.width;
        xx -= width / 2; //ortho counts from the center 
        float yy = pos.y * height / Screen.height;
        yy -= height / 2;

        return new Vector3(xx, yy, 0);
    }
}