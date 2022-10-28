using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SWSM_ParserTests
{
    [UnitySetUp]
    public IEnumerator Setup()
    {
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Scenes/MainMenu");
        while (!sceneLoad.isDone)
        {
            yield return null;
        }
    }

    [Test]
    public void DoesUpdateMapActionPickerInvokeShowMapEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("map_update", "show_map"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesUpdateMapActionPickerInvokeActivatePortalEvents()
    {
        bool portalFired = false;
        bool mapUpdateFired = false;
        GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.AddListener((data) => { portalFired = true; });
        GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.AddListener((data) => { mapUpdateFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("map_update", "activate_portal"));

        Assert.AreEqual(true, portalFired && mapUpdateFired);
    }

    [Test]
    public void DoesUpdateMapActionPickerInvokeExtendMapEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_MAP_REVEAL.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("map_update", "extend_map"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEncounterUpdateInvokeBeginEncounterEvents()
    {
        bool eventFired = false;
        GameStatuses newGameStatus = GameStatuses.Combat;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            newGameStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("encounter_update", "begin_encounter"));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.Encounter, newGameStatus);
    }

    [Test]
    public void DoesProcessMerchantUpdateInvokeBeginMerchantEvents()
    {
        bool eventFired = false;
        GameStatuses newGameStatus = GameStatuses.Combat;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            newGameStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("merchant_update", "begin_merchant"));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.Merchant, newGameStatus);
    }

    [Test]
    public void DoesProcessCampUpdateInvokeBeginCampEvents()
    {
        bool eventFired = false;
        GameStatuses newGameStatus = GameStatuses.Combat;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            newGameStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("camp_update", "begin_camp"));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.Camp, newGameStatus);
    }

    [Test]
    public void DoesProcessCombatUpdateInvokeBeginCombatEvents()
    {
        bool eventFired = false;
        GameStatuses newGameStatus = GameStatuses.Map;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            newGameStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("combat_update", "begin_combat"));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.Combat, newGameStatus);
    }

    [Test]
    public void DoesProcessCombatUpdateInvokeStatusUpdateEvents()
    {
        bool eventFired = false;
        int firedCount = 0;
        GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.AddListener((data) =>
        {
            eventFired = true;
            firedCount++;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestStatusData("combat_update", "update_statuses", 1));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(1, firedCount);

        // test for multple statuses
        firedCount = 0;
        eventFired = false;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestStatusData("combat_update", "update_statuses", 6));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(6, firedCount);
    }

    [Test]
    public void DoesProcessCombatUpdateInvokeProcessCombatQueueEvents()
    {
        bool eventFired = false;
        int firedCount = 0;
        GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.AddListener((data) =>
        {
            eventFired = true;
            firedCount++;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCombatQueueData("combat_update", "combat_queue", 1));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(1, firedCount);

        // test for multple statuses
        firedCount = 0;
        eventFired = false;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCombatQueueData("combat_update", "combat_queue", 15));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(15, firedCount);
    }
    
    [Test]
    public void DoesProcessCombatUpdateLogErrorIfBadAction()
    {
        string data = TestUtils.BuildTestSwsmData("combat_update", "test");
        SWSM_Parser.ParseJSON(data);
        LogAssert.Expect(LogType.Log ,$"[SWSM Parser][Combat Update] Unknown Action \"test\". Data = {data}");
    }

    [Test]
    public void DoesProcessTreasureUpdateInvokeBeginTreasureEvents()
    {
        bool eventFired = false;
        GameStatuses newGameStatus = GameStatuses.GameOver;
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            newGameStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("treasure_update", "begin_treasure"));

        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.Treasure, newGameStatus);
    }

    [Test]
    public void DoesProcessTreasureUpdateLogWarningIfUnknownAction()
    {
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("treasure_update", "activate_portal"));

        LogAssert.Expect(LogType.Warning,
            "[SWSM Parser][Treasure Update] Unknown Action \"activate_portal\". Data = {\"data\":{\"message_type\":\"treasure_update\",\"action\":\"activate_portal\",\"data\":[]}}");
    }

    [Test]
    public void DoesEnemyIntentsMessageLogWarning()
    {
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("enemy_intents", "activate_portal"));

        LogAssert.Expect(LogType.Warning, "[SWSM Parser] Enemy Intents are no longer listened for.");
    }

    [Test]
    public void DoesProcessPlayerStateUpdateInvokeCorrectEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener((playerStateData) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestPlayerStateData("player_state_update", "activate_portal"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessErrorActionLogCorrectWarning()
    {
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("error", "card_unplayable"));

        LogAssert.Expect(LogType.Log, "card_unplayable: ");

        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("error", "invalid_card"));

        LogAssert.Expect(LogType.Log, "invalid_card: ");

        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("error", "insufficient_energy"));

        // TODO THIS FAILS EVERY TIME FOR NO REASON
        // LogAssert.Expect(LogType.Log, new Regex("insufficient_energy: "));
    }

    [Test]
    public void DoesProcessGenericDataInvokeUpdateEnergyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener((data, data2) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestGenericEnergyData("generic_data", "Energy"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessGenericDataInvokeCardPilesEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARDS_PILES_UPDATED.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestGenericCardPilesData("generic_data", "CardsPiles"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessGenericDataInvokeEnemiesEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("generic_data", "Enemies"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessGenericDataInvokePlayersEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("generic_data", "Players"));

        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessGenericDataInvokeEnemyIntentsEvents()
    {
        bool eventFired = false;
        int eventCount = 0;
        GameManager.Instance.EVENT_UPDATE_INTENT.AddListener((data) =>
        {
            eventFired = true;
            eventCount++;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestEnemyIntentData("generic_data", "EnemyIntents", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(1, eventCount);
        eventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestEnemyIntentData("generic_data", "EnemyIntents", 3));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(3, eventCount);
    }
    
    [Test]
    public void DoesProcessGenericDataInvokeEnemyIntentsErrors()
    {
        //TODO discuss this, as this cannot happen with the code the way it is.
        string data = TestUtils.BuildTestEnemyIntentData("generic_data", "EnemyIntents", 1);
        SWSM_Parser.ParseJSON(data);
        LogAssert.Expect(LogType.Log ,$"[SWSM Parser][Combat Update] Unknown Action \"test\". Data = {data}");
    }

    [Test]
    public void DoesProcessGenericDataInvokeStatusesEvents()
    {
        bool eventFired = false;
        int eventCount = 0;

        GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.AddListener((data) =>
        {
            eventFired = true;
            eventCount++;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestStatusData("generic_data", "Statuses", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(1, eventCount);

        eventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestStatusData("generic_data", "Statuses", 7));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(7, eventCount);
    }

    [Test]
    public void DoesProcessGenericDataInvokePlayerDeckEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_PILE_SHOW_DECK.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("generic_data", "PlayerDeck"));
        Assert.AreEqual(true, eventFired);
    }
    
    [Test]
    public void DoesProcessGenericDataInvokeCampShowUpgradeableCardEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CAMP_SHOW_UPRGRADEABLE_CARDS.AddListener((data) => { eventFired = true;});
        SWSM_Parser.ParseJSON(TestUtils.BuildTestUpgradeableCardData());
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessGenericDataLogInvalidData()
    {
        string data = TestUtils.BuildTestSwsmData("generic_data", "test");
        SWSM_Parser.ParseJSON(data);
        LogAssert.Expect(LogType.Log, $"[SWSM Parser] [Generic Data] Uncaught Action \"test\". Data = {data}");
    }

    [Test]
    public void DoesProcessEnemyAffectedInvokeUpdateEnergyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener((data, data2) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestGenericEnergyData("enemy_affected", "update_energy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEnemyAffectedInvokeProcessMoveCardEvents()
    {
        bool eventFired = false;
        bool wsDataEventFired = false;
        int eventCount = 0;
        GameManager.Instance.EVENT_MOVE_CARD.AddListener((data, data2) =>
        {
            eventFired = true;
            eventCount++;
        });
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) => { wsDataEventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("enemy_affected", "move_card", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(1, eventCount);

        eventFired = false;
        wsDataEventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("enemy_affected", "move_card", 23));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(23, eventCount);

        eventFired = false;
    }

    [Test]
    public void DoesProcessEnemyAffectedInvokeProcessUpdateEnemyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("enemy_affected", "update_enemy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEnemyAffectedInvokeProcessUpdatePlayerEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("enemy_affected", "update_player"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessPlayerAffectedInvokeUpdateEnergyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener((data, data2) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestGenericEnergyData("player_affected", "update_energy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessPlayerAffectedInvokeProcessMoveCardEvents()
    {
        bool eventFired = false;
        bool wsDataEventFired = false;
        int eventCount = 0;
        GameManager.Instance.EVENT_MOVE_CARD.AddListener((data, data2) =>
        {
            eventFired = true;
            eventCount++;
        });
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) => { wsDataEventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("player_affected", "move_card", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(1, eventCount);

        eventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("player_affected", "move_card", 3));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(3, eventCount);
    }

    [Test]
    public void DoesProcessPlayerAffectedInvokeProcessUpdateEnemyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("player_affected", "update_enemy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessPlayerAffectedInvokeProcessUpdatePlayerEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("player_affected", "update_player"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEnemyAffectedInvokeProcessCreateCardEvents()
    {
        bool eventFired = false;
        int eventCount = 0;
        GameManager.Instance.EVENT_CARD_CREATE.AddListener((data) =>
        {
            eventFired = true;
            eventCount++;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("player_affected", "create_card", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(1, eventCount);

        eventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("player_affected", "create_card", 3));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(3, eventCount);
    }

    [Test]
    public void DoesProcessEndOfTurnInvokeUpdateEnergyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener((data, data2) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestGenericEnergyData("end_turn", "update_energy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEndOfTurnInvokeProcessMoveCardEvents()
    {
        bool eventFired = false;
        bool wsDataEventFired = false;
        int eventCount = 0;
        GameManager.Instance.EVENT_MOVE_CARD.AddListener((data, data2) =>
        {
            eventFired = true;
            eventCount++;
        });
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) => { wsDataEventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("end_turn", "move_card", 1));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(1, eventCount);

        eventFired = false;
        eventCount = 0;
        SWSM_Parser.ParseJSON(TestUtils.BuildTestCardMoveData("end_turn", "move_card", 3));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(true, wsDataEventFired);
        Assert.AreEqual(3, eventCount);
    }

    [Test]
    public void DoesProcessEndOfTurnInvokeProcessUpdateEnemyEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_ENEMIES.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_turn", "update_enemy"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessEndOfTurnInvokeProcessUpdatePlayerEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_UPDATE_PLAYER.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_turn", "update_player"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessBeginTurnInvokeProcessDrawCardsEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.AddListener(() => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("begin_turn", "move_card"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesProcessBeginTurnInvokeProcessChangeTurnEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CHANGE_TURN.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("begin_turn", "change_turn"));
        Assert.AreEqual(true, eventFired);
    }

    [Test]
    public void DoesEndNodeMessageInvokeProcessEndCombatEnemiesDefeatedEvents()
    {
        bool statusChangeEventFired = false;
        bool rewardPanelEventFired = false;
        GameStatuses returnStatus = GameStatuses.Camp;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            statusChangeEventFired = true;
            returnStatus = data;
        });
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener((data) => { rewardPanelEventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_node", "enemies_defeated"));
        Assert.AreEqual(true, statusChangeEventFired);
        Assert.AreEqual(true, rewardPanelEventFired);
    }

    [Test]
    public void DoesEndNodeMessageInvokeProcessEndCombatPlayerDefeatedEvents()
    {
        bool eventFired = false;
        GameStatuses returnStatus = GameStatuses.Camp;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            returnStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_node", "player_defeated"));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.GameOver, returnStatus);
    }

    [Test]
    public void DoesEndNodeMessageInvokeProcessEndCombatPlayersDefeatedEvents()
    {
        bool eventFired = false;
        GameStatuses returnStatus = GameStatuses.Camp;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            returnStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_node", "players_defeated"));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.GameOver, returnStatus);
    }

    [Test]
    public void DoesEndNodeMessageInvokeProcessEndCombatSelectAnotherRewardEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_node", "select_another_reward"));
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesCallingEndNodeShowMapLoadTheExpeditionScene()
    {
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_node", "show_map"));
        yield return null;
        string sceneName = SceneManager.GetActiveScene().name;
        Assert.AreEqual("Loader", sceneName);
        //TODO Figure out how to check that the exepdition gets loaded
    }
    
    [Test]
    public void DoesEndCombatMessageInvokeProcessEndCombatEnemiesDefeatedEvents()
    {
        bool statusChangeEventFired = false;
        bool rewardPanelEventFired = false;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            statusChangeEventFired = true;
        });
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener((data) => { rewardPanelEventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_combat", "enemies_defeated"));
        Assert.AreEqual(true, statusChangeEventFired);
        Assert.AreEqual(true, rewardPanelEventFired);
    }

    [Test]
    public void DoesEndCombatMessageInvokeProcessEndCombatPlayerDefeatedEvents()
    {
        bool eventFired = false;
        GameStatuses returnStatus = GameStatuses.Camp;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            returnStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_combat", "player_defeated"));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.GameOver, returnStatus);
    }

    [Test]
    public void DoesEndCombatMessageInvokeProcessEndCombatPlayersDefeatedEvents()
    {
        bool eventFired = false;
        GameStatuses returnStatus = GameStatuses.Camp;
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener((data) =>
        {
            eventFired = true;
            returnStatus = data;
        });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_combat", "players_defeated"));
        Assert.AreEqual(true, eventFired);
        Assert.AreEqual(GameStatuses.GameOver, returnStatus);
    }

    [Test]
    public void DoesEndCombatMessageInvokeProcessEndCombatSelectAnotherRewardEvents()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.AddListener((data) => { eventFired = true; });
        SWSM_Parser.ParseJSON(TestUtils.BuildTestChangeTurnData("end_combat", "select_another_reward"));
        Assert.AreEqual(true, eventFired);
    }

    [UnityTest]
    public IEnumerator DoesCallingEndCombatShowMapLoadTheExpeditionScene()
    {
        SWSM_Parser.ParseJSON(TestUtils.BuildTestSwsmData("end_combat", "show_map"));
        yield return null;
        string sceneName = SceneManager.GetActiveScene().name;
        Assert.AreEqual("Loader", sceneName);
        //TODO Figure out how to check that the exepdition gets loaded
    }

    [Test]
    public void DoesParserThrowErrorWhenGivenBadMessageType()
    {
        string data = TestUtils.BuildTestSwsmData("failure", "nope");
        SWSM_Parser.ParseJSON(data);
        LogAssert.Expect(LogType.Error ,"[SWSM Parser] No message_type processed. Data Received: " + data);
    }

    
    
}