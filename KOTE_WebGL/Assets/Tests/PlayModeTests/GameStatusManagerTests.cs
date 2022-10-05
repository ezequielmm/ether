using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using map;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameStatusManagerTests : MonoBehaviour
{
    private GameStatusManager _gameStatusManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return null;
        GameObject mapPrefab = new GameObject();
        _gameStatusManager = mapPrefab.AddComponent<GameStatusManager>();
        mapPrefab.SetActive(true);
        _gameStatusManager = mapPrefab.GetComponent<GameStatusManager>();
        mapPrefab.SetActive(true);
        yield return null;
    }


    [Test]
    public void GameSetToMapStatusOnStart()
    {
        Assert.AreEqual(GameStatuses.Map, _gameStatusManager.currentGameStatus);
    }

    [Test]
    public void DoesNodeDataUpdateTriggerCombatStatus()
    {
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(null, WS_QUERY_TYPE.CARD_PLAYED);
        Assert.AreEqual(GameStatuses.Map, _gameStatusManager.currentGameStatus);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(null, WS_QUERY_TYPE.MAP_NODE_SELECTED);
        Assert.AreEqual(GameStatuses.Combat, _gameStatusManager.currentGameStatus);
    }

    [Test]
    public void DoesPrepareStatusChangeActivateGameAboutToEnd()
    {
        // this is very similar to testing event confirmation, as we need to test the status of a private variable
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dying");
        Assert.True(eventFired);
    }

    [Test]
    public void DoesConfirmEventFireConfirmEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dying");
        Assert.True(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dead");
        Assert.False(eventFired);
    }

    [Test]
    public void DoesConfirmEventFireGameStatusChange()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke("dead");
        Assert.True(eventFired);
    }

    [Test]
    public void DoesGameStatusChangeToCombatToggleTopBarMapIconOn()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToCombatToggleMapPanelOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToCombatToggleCombatElementsOn()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToCombatToggleTreasureElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToCombatTriggerDrawingCards()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(() => { eventFired = true; });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        Assert.True(eventFired);
    }

    [Test]
    public void DoesGameStatusChangeToMapToggleTopBarMapIconOff()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Map);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToMapToggleMapPanelOn()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Map);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToMapToggleCombatElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Map);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToMapToggleTreasureElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Map);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
     [Test]
    public void DoesGameStatusChangeToTreasureToggleTopBarMapIconOn()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToTreasureToggleMapPanelOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToTreasureToggleCombatElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToTreasureToggleTreasureElementsOn()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToEncounterToggleTopBarMapIconOn()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToEncounterToggleMapPanelOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToEncounterToggleCombatElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToEncounterShowEncounterPanel()
    {
        bool eventFired = false;

        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.AddListener(() =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesGameStatusChangeToMerchantToggleTopBarMapIconOn()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToMerchantToggleMapPanelOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToMerchantToggleCombatElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToMerchantShowMerchantPanel()
    {
        bool eventFired = false;

        GameManager.Instance.EVENT_SHOW_MERCHANT_PANEL.AddListener(() =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesGameStatusChangeToCampToggleTopBarMapIconOn()
    {
        bool eventFired = false;
        bool correctStatus = false;
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToCampToggleMapPanelOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }
    
    [Test]
    public void DoesGameStatusChangeToCampToggleCombatElementsOff()
    {
        bool eventFired = false;
        bool correctStatus = false;

        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.AddListener((toggleStatus) =>
        {
            eventFired = true;
            if (!toggleStatus) correctStatus = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        Assert.True(eventFired);
        Assert.True(correctStatus);
    }

    [Test]
    public void DoesGameStatusChangeToCampShowCampPanel()
    {
        bool eventFired = false;

        GameManager.Instance.EVENT_SHOW_CAMP_PANEL.AddListener(() =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        Assert.True(eventFired);
    }
}