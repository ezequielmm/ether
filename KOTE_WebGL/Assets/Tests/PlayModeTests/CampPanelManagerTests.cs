using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class CampPanelManagerTests : MonoBehaviour
{
    private CampPanelManager _campPanelManager;
    private GameObject spriteManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        GameObject CampPanelPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NonCombatNodes/CampPanel.prefab");
        GameObject campPanelGo = Instantiate(CampPanelPrefab);
        _campPanelManager = campPanelGo.GetComponent<CampPanelManager>();
        campPanelGo.SetActive(true);
        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_campPanelManager.gameObject);
        Destroy(spriteManager);
        yield return null;
    }

    [Test]
    public void IsCampPanelDeactivatedOnStart()
    {
        Assert.False(_campPanelManager.campContainer.activeSelf);
    }

    [Test]
    public void DoesShowCampPanelEventActivateCampPanel()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
        Assert.True(_campPanelManager.campContainer.activeSelf);
    }

    [Test]
    public void DoesShowCampPanelToggleCombatElements()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesShowCampPanelToggleCombatElementsOff()
    {
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((data) =>
        {
            if (data == false) correctStatus = true;
        });
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesSelectingRestFireCampHealEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CAMP_HEAL.AddListener(() => { eventFired = true; });
        _campPanelManager.OnRestSelected();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesSelectingRestDeactivateCampButtons()
    {
        Assert.True(_campPanelManager.restButton.interactable);
        Assert.True(_campPanelManager.smithButton.interactable);
        _campPanelManager.OnRestSelected();
        Assert.False(_campPanelManager.restButton.interactable);
        Assert.False(_campPanelManager.smithButton.interactable);
    }

    [Test]
    public void DoesSelectingRestSwitchMainButtonToContinue()
    {
        _campPanelManager.OnRestSelected();
        Assert.AreEqual("Continue", _campPanelManager.skipButtonText.text);
    }

    [Test]
    public void DoesSelectingSkipFireContinueExpeditionEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.AddListener(() => { eventFired = true; });
        _campPanelManager.OnSkipSelected();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesSelectingSkipDeactivateCampContainer()
    {
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
        Assert.True(_campPanelManager.campContainer.activeSelf);
        _campPanelManager.OnSkipSelected();
        Assert.False(_campPanelManager.campContainer.activeSelf);
    }

    [Test]
    public void DoesOnShowCardUpgradePanelFireShowDirectSelectCardEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SHOW_DIRECT_SELECT_CARD_PANEL.AddListener((deck, options, select) =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_SHOW_UPGRADE_CARDS_PANEL.Invoke(new Deck { cards = new List<Card>() });
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnShowCardUpgradePanelSendCorrectSelectOptions()
    {
        int hideBackButton = -1;
        int mustSelectAllCards = -1;
        int numberOfCardsToSelect = -1;
        int fireSelectWhenCardClicked = -1;
        GameManager.Instance.EVENT_SHOW_DIRECT_SELECT_CARD_PANEL.AddListener((deck, options, select) =>
        {
            if (options.HideBackButton == false) hideBackButton = 0;
            if (options.MustSelectAllCards == true) mustSelectAllCards = 1;
            numberOfCardsToSelect = options.NumberOfCardsToSelect;
            if (options.FireSelectWhenCardClicked) fireSelectWhenCardClicked = 1;
        });
        GameManager.Instance.EVENT_SHOW_UPGRADE_CARDS_PANEL.Invoke(new Deck { cards = new List<Card>() });
        Assert.AreEqual(0, hideBackButton);
        Assert.AreEqual(1, mustSelectAllCards);
        Assert.AreEqual(1, numberOfCardsToSelect);
        Assert.AreEqual(1, fireSelectWhenCardClicked);
    }

    [Test]
    public void DoesCampFinishEventDeactivateCampButtons()
    {
        Assert.True(_campPanelManager.restButton.interactable);
        Assert.True(_campPanelManager.smithButton.interactable);
        GameManager.Instance.EVENT_CAMP_FINISH.Invoke();
        Assert.False(_campPanelManager.restButton.interactable);
        Assert.False(_campPanelManager.smithButton.interactable);
    }

    [Test]
    public void DoesCampFinishEventSwitchMainButtonToContinue()
    {
        GameManager.Instance.EVENT_CAMP_FINISH.Invoke();
        Assert.AreEqual("Continue", _campPanelManager.skipButtonText.text);
    }
    
    
}