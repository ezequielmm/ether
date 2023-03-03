using System.Collections;
using NUnit.Framework;
using UnityEngine;
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
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(_gameStatusManager.gameObject);
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
    public void DoesEventConfirmationWithPlayerStateActivateToggleGameClick()
    {
        // this is very similar to testing event confirmation, as we need to test the status of a private variable
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState),"dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesEventConfirmationWithEnemyStateActivateToggleGameClick()
    {
        // this is very similar to testing event confirmation, as we need to test the status of a private variable
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState),"dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesEventConfirmationWithPlayerStateActivateGameAboutToEndOnlyOnGameOverStatus()
    {
        // this is very similar to testing event confirmation, as we need to test the status of a private variable
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");        
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RoyalHouse);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesEventConfirmationWithEnemyStateActivateGameAboutToEndOnlyOnGameOverStatus()
    {
        // this is very similar to testing event confirmation, as we need to test the status of a private variable
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");        
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RoyalHouse);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.False(eventFired);
        
        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");
        Assert.True(eventFired);
    }

    [Test]
    public void DoesConfirmEventWithPlayerStatusFireConfirmEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dying");   
        Assert.True(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState), "dead");
        Assert.False(eventFired);
    }
    
    [Test]
    public void DoesConfirmEventWithEnemyStatusFireConfirmEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dying");   
        Assert.True(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState), "dead");
        Assert.False(eventFired);
    }

    [Test]
    public void DoesConfirmEventWithPlayerStateFireGameStatusChange()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState),"dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState),"dead");
        Assert.True(eventFired);
    }
    
    [Test]
    public void DoesConfirmEventWithEnemyStateFireGameStatusChange()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) => { eventFired = true; });

        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState),"dying");
        Assert.False(eventFired);

        eventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState),"dead");
        Assert.True(eventFired);
    }

    [Test]
    public void DoesConfirmEventWithPlayerStateSwitchToGameOver()
    {
        bool eventFired = false;
        GameStatuses newStatus = GameStatuses.None;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) => { eventFired = true;
            newStatus = data;
        });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState),"dead");
        Assert.False(eventFired);
        Assert.AreNotEqual(GameStatuses.GameOver, newStatus);

        eventFired = false;
        newStatus = GameStatuses.None;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState),"dead");
        Assert.AreEqual(GameStatuses.GameOver, newStatus);
    }
    
    [Test]
    public void DoesConfirmEventWithEnemyStateSwitchToRewardsPanel()
    {
        bool eventFired = false;
        GameStatuses newStatus = GameStatuses.None;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) => { eventFired = true;
            newStatus = data;
        });
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(PlayerState),"dead");
        Assert.False(eventFired);
        Assert.AreNotEqual(GameStatuses.RewardsPanel, newStatus);

        eventFired = false;
        newStatus = GameStatuses.None;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
        GameManager.Instance.EVENT_CONFIRM_EVENT.Invoke(typeof(EnemyState),"dead");
        Assert.AreEqual(GameStatuses.RewardsPanel, newStatus);
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

    [UnityTest]
    public IEnumerator DoesGameStatusChangeToCombatTriggerDrawingCards()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(() => { eventFired = true; });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
        yield return new WaitForSeconds(0.22f);
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
    public void DoesGameStatusChangeToTreasureOnlyShowPlayer()
    {
        bool eventFired = false;

        GameManager.Instance.EVENT_SHOW_PLAYER_CHARACTER.AddListener(() =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
        Assert.True(eventFired);
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

        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.AddListener((data) =>
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

        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.AddListener(() =>
        {
            eventFired = true;
        });
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
        Assert.True(eventFired);
    }
}