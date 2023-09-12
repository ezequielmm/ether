using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms.Impl;


public class WebSocketParser
{
    public static void ParseJSON(string data)
    {
        SWSM_Base swsm = JsonConvert.DeserializeObject<SWSM_Base>(data);

        switch (swsm.data.message_type)
        {
            case nameof(WS_MESSAGE_TYPES.map_update):
                UpdateMapActionPicker(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.encounter_update):
                ProcessEncounterUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.merchant_update):
                ProcessMerchantUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.camp_update):
                ProcessCampUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.card_upgrade):
                Debug.LogError($"{WS_MESSAGE_TYPES.card_upgrade} is not implemented, report this.");
                throw new NotImplementedException();
                break;
            case nameof(WS_MESSAGE_TYPES.add_potion):
                ProcessAddPotion(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.use_potion):
                ProcessUsePotion(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.add_trinket):
                ProcessAddTrinket(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.combat_update):
                ProcessCombatUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.treasure_update):
                ProcessTreasureUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.end_treasure):
                ProcessEndTreasureUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.enemy_intents):
                //ProcessEnemyIntents(swsm.data.action, data);
                Debug.LogWarning($"[SWSM Parser] Enemy Intents are no longer listened for.");
                break;
            case nameof(WS_MESSAGE_TYPES.player_state_update):
                ProcessPlayerStateUpdate(data);
                break;
            case nameof(WS_MESSAGE_TYPES.error):
                ProcessErrorAction(swsm.data.action, data);
                break;
            //Data types
            case nameof(WS_MESSAGE_TYPES.generic_data):
                ProcessGenericData(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.enemy_affected):
                ProcessEnemyAffected(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.player_affected):
                ProcessPlayerAffected(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.end_turn):
                ProcessEndOfTurn(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.begin_turn):
                ProcessBeginTurn(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.end_node):
            case nameof(WS_MESSAGE_TYPES.end_combat):
                ProcessEndCombat(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.card_updated):
                ProcessCardUpdated(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.trinket_triggered):
                ProcessTrinketTriggered(swsm.data.action, data);
                break;
            default:
                Debug.LogWarning("[SWSM Parser] Unknown message_type: " + swsm.data.message_type + " , data:" + data);
                break;
        }
    }

    #region WS_MESSAGE_TYPES_processors

    private static void UpdateMapActionPicker(string action, string data)
    {
#if UNITY_EDITOR
        if (GameSettings.DEBUG_MODE_ON)
        {
            data = Utils.ReadJsonFile("map_test_data.json");
        }
#endif

        SWSM_MapData mapData = JsonConvert.DeserializeObject<SWSM_MapData>(data);
        switch (action)
        {
            case "show_map":
                GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(mapData);
                break;
            case "activate_portal":
                GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.Invoke(mapData);
                GameManager.Instance.EVENT_ALL_MAP_NODES_UPDATE.Invoke(mapData);
                break;
            case "extend_map":
                GameManager.Instance.EVENT_MAP_REVEAL.Invoke(mapData);
                break;
            default:
                Debug.LogWarning("[UpdateMapActionPicker] unknown action: " + action + " , data: " + data);
                break;
        }
    }


    private static void ProcessCombatUpdate(string action, string data)
    {
        switch (action)
        {
            case "begin_combat":
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
                break;
            case "update_statuses":
                ProcessStatusUpdate(data);
                break;
            case "combat_queue":
                ProcessCombatQueue(data);
                break;
            case "show_card_dialog":
                ProcessShowCardDialog(data);
                break;
            case "spawn_enemies":
                ProcessSpawnEnemies(data);
                break;
            default:
                Debug.LogWarning($"[SWSM Parser][Combat Update] Unknown Action \"{action}\". Data = {data}");
                break;
        }
    }


    private static void ProcessPlayerStateUpdate(string data)
    {
        SWSM_PlayerState playerStateBase = JsonConvert.DeserializeObject<SWSM_PlayerState>(data);

        PlayerStateData playerState = playerStateBase.data;

        Debug.Log($"Health Update is :{playerState.data.playerState.hpCurrent}/{playerState.data.playerState.hpMax}");
        GameManager.Instance.PlayerStateUpdate(playerState);
    }


    private static void ProcessErrorAction(string action, string data)
    {
        SWSM_ErrorData errorData;
        //TODO this will need to get passed on to where it needs to go once we determine what the error data will be
        switch (action)
        {
            case nameof(WS_ERROR_TYPES.card_unplayable):
                errorData = JsonUtility.FromJson<SWSM_ErrorData>(data);
                Debug.Log(action + ": " + errorData.data);
                break;
            case nameof(WS_ERROR_TYPES.invalid_card):
                errorData = JsonUtility.FromJson<SWSM_ErrorData>(data);
                Debug.Log(action + ": " + errorData.data);
                break;
            case nameof(WS_ERROR_TYPES.insufficient_energy):
                errorData = JsonUtility.FromJson<SWSM_ErrorData>(data);
                Debug.Log(action + ": " + errorData.data);
                break;
        }
    }

    /// <summary>
    /// This data is coming from backend after a request from frontend
    /// </summary>
    /// <param name="action"></param>
    /// <param name="data"></param>
    private static void ProcessGenericData(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_DATA_REQUEST_TYPES.Energy):
                UpdateEnergy(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.CardsPiles):

                SWSM_CardsPiles deck = JsonConvert.DeserializeObject<SWSM_CardsPiles>(data);
                Debug.Log($"[SWSM Parser] CardPiles data => {data}");
                Debug.Log(
                    $"Cards Pile Counts: [Draw] {deck.data.data.draw.Count} | [Hand] {deck.data.data.hand.Count} " +
                    $"| [Discard] {deck.data.data.discard.Count} | [Exhaust] {deck.data.data.exhausted.Count}");

                GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(deck.data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Enemies):

                // SWSM_Enemies enemies = JsonConvert.DeserializeObject<SWSM_Enemies>(data);
                //   GameManager.Instance.EVENT_UPDATE_ENEMIES.Invoke(enemies.data);
                ProcessUpdateEnemy(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Players):
                ProcessUpdatePlayer(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.EnemyIntents):
                ProcessEnemyIntents(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Statuses):
                ProcessStatusUpdate(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.PlayerDeck):
                ProcessPlayerFullDeck(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.UpgradableCards):
                throw new NotImplementedException();
            case nameof(WS_DATA_REQUEST_TYPES.MerchantData):
                throw new NotImplementedException();
            case nameof(WS_DATA_REQUEST_TYPES.TreasureData):
                ProcessTreasureData(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.EncounterData):
                Debug.LogWarning($"Using Obsolete Method. Please don't use a generic, encounterData request.");
                GameObject.FindObjectOfType<EncounterManager>().ShowAndPopulate();
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Rewards):
                ProcessRewardsData(data);
                break;
            case "chest_result":
                ProcessChestResult(data);
                break;
            default:
                Debug.Log($"[SWSM Parser] [Generic Data] Uncaught Action \"{action}\". Data = {data}");
                break;
        }
    }


    private static void ProcessEnemyAffected(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.update_energy):
                UpdateEnergy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.move_card):
                ProcessMoveCard(data);
                break;

            case nameof(WS_MESSAGE_ACTIONS.update_enemy):
                ProcessUpdateEnemy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.update_player):
                ProcessUpdatePlayer(data);
                break;
        }
    }

    private static void ProcessPlayerAffected(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.update_energy):
                UpdateEnergy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.move_card):
                ProcessMoveCard(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.update_enemy):
                ProcessUpdateEnemy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.update_player):
                ProcessUpdatePlayer(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.add_card):
                ProcessAddCard(data);
                break;
            default:
                Debug.LogWarning("[ProcessPlayerAffected] unknown action: " + action + " , data: " + data);
                break;
        }
    }

    private static void ProcessEndOfTurn(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.update_energy):
                UpdateEnergy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.move_card):
                ProcessMoveCard(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.update_enemy):
                ProcessUpdateEnemy(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.update_player):
                ProcessUpdatePlayer(data);
                break;
            default:
                Debug.LogWarning("[ProcessEndOfTurn] unknown action: " + action + " , data: " + data);
                break;
        }
    }

    private static void ProcessBeginTurn(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.move_card):
                Debug.Log("Should move cards from draw to hand");
                ProcessDrawCards(data);
                break;
            case nameof(WS_MESSAGE_ACTIONS.change_turn):
                ProcessChangeTurn(data);
                break;
        }
    }

    private static void ProcessEndCombat(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.enemies_defeated):
                Debug.Log("Should move cards from draw to hand");
                GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
                break;
            case nameof(WS_MESSAGE_ACTIONS.player_defeated):
            case nameof(WS_MESSAGE_ACTIONS.players_defeated):
                Debug.Log("GAME OVER");
                GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
                break;
            case nameof(WS_MESSAGE_ACTIONS.show_rewards):
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.RewardsPanel);
                break;
            
            case nameof(WS_MESSAGE_ACTIONS.select_another_reward):
                SWSM_RewardsData updatedRewardsData = JsonConvert.DeserializeObject<SWSM_RewardsData>(data);
                GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(updatedRewardsData);
                break;
            case nameof(WS_MESSAGE_ACTIONS.show_score):
                GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.ScoreBoard);
                break;
            case nameof(WS_MESSAGE_ACTIONS.show_next_stage):
                GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.Invoke(GameStatuses.ScoreBoardAndNextAct);
                break;
            case nameof(WS_MESSAGE_ACTIONS.show_map):
                // Change to Loader Scene which will load the Map Scene
                GameManager.Instance.LoadScene(inGameScenes.Expedition);
                EnemyManager.ClearCache();
                break;

            default:
                Debug.LogWarning("[ProcessEndCombat] unknown action: " + action + " , data: " + data);
                break;
        }
    }

    private static void ProcessCardUpdated(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.update_card_description):
                SWSM_CardUpdateData updateData = JsonConvert.DeserializeObject<SWSM_CardUpdateData>(data);
                GameManager.Instance.EVENT_CARD_UPDATE_TEXT.Invoke(updateData.data.data.card);
                break;
            default:
                Debug.LogWarning("[ProcessCardUpdated] unknown action: " + action + " , data: " + data);
                break;
        }
    }

    private static void ProcessTrinketTriggered(string action, string data)
    {
        switch (action)
        {
            case "flash_trinket_icon":
                TrinketTriggeredData trinketData = JsonConvert.DeserializeObject<TrinketTriggeredData>(data);
                GameManager.Instance.EVENT_TRINKET_ACTIVATED.Invoke(trinketData.data.trinket);
                break;
            default:
                Debug.LogWarning("[ProcessTrinketTriggered] unknown action: " + action + " , data: " + data);
                break;
        }
    }

    #endregion

    #region actionProcessors

    private static void ProcessStatusUpdate(string data)
    {
        SWSM_StatusData statusData = JsonConvert.DeserializeObject<SWSM_StatusData>(data);
        Debug.Log(
            $"[SWSM_Parser][ProcessStatusUpdate] Source --> [ {statusData.data.message_type} | {statusData.data.action} ] {data}");
        List<StatusData> statuses = statusData.data.data;
        foreach (StatusData status in statuses)
        {
            GameManager.Instance.EVENT_UPDATE_STATUS_EFFECTS.Invoke(status);
        }
    }


    private static void ProcessCombatQueue(string data)
    {
        Debug.Log($"ProcessCombatQueue data: {data}");
        SWSM_CombatAction combatAction = JsonConvert.DeserializeObject<SWSM_CombatAction>(data);
        Debug.Log($"[SWSM Parser] Combat Queue Data: {data}");
        foreach (CombatTurnData combatData in combatAction.data.data) // For when it's a list.
        {
            GameManager.Instance.EVENT_COMBAT_TURN_ENQUEUE.Invoke(combatData);
        }
    }

    private static void ProcessShowCardDialog(string data)
    {
        SWSM_ShowCardDialog showCards = JsonConvert.DeserializeObject<SWSM_ShowCardDialog>(data);
        Debug.Log($"[SWSM Parser] [Show Card Dialog] data: {data}");
        if (showCards.data.data.cards == null || showCards.data.data.cards.Count == 0)
        {
            GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT.Invoke("Not enough cards on pile");
            return;
        }

        SelectPanelOptions panelOptions = new SelectPanelOptions
        {
            HideBackButton = true,
            MustSelectAllCards = false,
            NumberOfCardsToSelect = showCards.data.data.cardsToTake,
            ShowCardInCenter = true
        };
        GameManager.Instance.EVENT_SHOW_SELECT_CARD_PANEL.Invoke(showCards.data.data.cards,
            panelOptions,
            (selectedCards) => { SendData.Instance.SendCardsSelected(selectedCards); });
    }

    private static void ProcessSpawnEnemies(string data)
    {
        SWSM_Enemies enemiesData = JsonConvert.DeserializeObject<SWSM_Enemies>(data);
        GameManager.Instance.EVENT_ADD_ENEMIES.Invoke(enemiesData.data);
    }

    private static void UpdateEnergy(string data)
    {
        SWSM_EnergyArray energyData = JsonConvert.DeserializeObject<SWSM_EnergyArray>(data);
        //        Debug.Log(energyData);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(energyData.data.data[0], energyData.data.data[1]);
    }

    private static void ProcessUpdateEnemy(string rawData)
    {
        SWSM_Enemies enemiesData = JsonConvert.DeserializeObject<SWSM_Enemies>(rawData);

        GameManager.Instance.EVENT_UPDATE_ENEMIES.Invoke(enemiesData.data);
    }

    private static void ProcessUpdatePlayer(string data)
    {
        SWSM_Players playersData = JsonConvert.DeserializeObject<SWSM_Players>(data);
        // foreach (PlayerData playerData in playersData.data)
        //  {
        GameManager.Instance.EVENT_UPDATE_PLAYER.Invoke(playersData.data.data);
        // }//TODO: plyersdta will be a list
    }

    private static void ProcessEnemyIntents(string data)
    {
        //Debug.Log($"[SWSM_Parser][ProcessEnemyIntents] data = {data}");
        SWSM_IntentData swsm_intentData = JsonConvert.DeserializeObject<SWSM_IntentData>(data);
        List<EnemyIntent> enemyIntents = swsm_intentData.data.data;
        foreach (EnemyIntent enemyIntent in enemyIntents)
        {
            if (enemyIntent != null)
                GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(enemyIntent);
        }
    }

    private static void ProcessPlayerFullDeck(string data)
    {
        SWSM_PlayerDeckData deckData = JsonConvert.DeserializeObject<SWSM_PlayerDeckData>(data);
        Deck deck = new Deck() { cards = deckData.data.data };
        GameManager.Instance.EVENT_CARD_PILE_SHOW_DECK.Invoke(deck);
    }

    private static void ProcessTreasureData(string data)
    {
        SWSM_TreasureData treasureData = JsonConvert.DeserializeObject<SWSM_TreasureData>(data);
        GameManager.Instance.EVENT_TREASURE_CHEST_SIZE.Invoke(treasureData);
    }

    private static void ProcessRewardsData(string data)
    {
        SWSM_RewardsData rewardsData = JsonConvert.DeserializeObject<SWSM_RewardsData>(data);
        
        GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(rewardsData);
    }

    private static void ProcessChestResult(string data)
    {
        SWSM_ChestResult chestResult = JsonConvert.DeserializeObject<SWSM_ChestResult>(data);
        GameManager.Instance.EVENT_TREASURE_CHEST_RESULT.Invoke(chestResult);
    }

    private static void ProcessMoveCard(string rawData)
    {
        SWSM_CardMove cardMoveData = JsonConvert.DeserializeObject<SWSM_CardMove>(rawData);
        Debug.Log($"[SWSM Parser] ProcessMoveCard [{cardMoveData.data.data.Length}]");
        int i = 0;
        List<(CardToMoveData, float)> cardMoveList = new List<(CardToMoveData, float)>();
        for (int y = cardMoveData.data.data.Length - 1; y >= 0; y--)
        {
            cardMoveList.Add((cardMoveData.data.data[y], i));
            i++;
        }

        GameManager.Instance.EVENT_MOVE_CARDS.Invoke(cardMoveList);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private static void ProcessDrawCards(string data)
    {
        //GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
    }

    private static void ProcessAddCard(string data)
    {
        SWSM_CardAdd cardAddData = JsonConvert.DeserializeObject<SWSM_CardAdd>(data);
        foreach (AddCardData addCardData in cardAddData.data.data)
        {
            GameManager.Instance.EVENT_CARD_ADD.Invoke(addCardData);
        }
    }

    private static void ProcessChangeTurn(string data)
    {
        SWSM_ChangeTurn who = JsonConvert.DeserializeObject<SWSM_ChangeTurn>(data);

        Debug.Log("[ProcessChangeTurn]data= " + data);
        Debug.Log("[ProcessChangeTurn]who.data= " + who.data);
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke(who.data.data);
    }

    #endregion


    //*****
    // NON Combat updates
    //********
    private static void ProcessTreasureUpdate(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_TREASURE_ACTIONS.begin_treasure):
            case nameof(WS_TREASURE_ACTIONS.continue_treasure):
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Treasure);
                break;
            default:
                Debug.LogWarning($"[SWSM Parser][Treasure Update] Unknown Action \"{action}\". Data = {data}");
                break;
        }
    }

    private static void ProcessEndTreasureUpdate(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.select_another_reward):
                SWSM_RewardsData updatedRewardsData = JsonConvert.DeserializeObject<SWSM_RewardsData>(data);
                GameManager.Instance.EVENT_POPULATE_REWARDS_PANEL.Invoke(updatedRewardsData);
                break;
        }
    }

    private static void ProcessEncounterUpdate(string action, string data)
    {
        switch (action)
        {
            case "begin_encounter":
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Encounter);
                break;
            case "show_cards":
                var encounterSelection = FetchData.ParseJsonWithPath<EncounterSelectionData>(data, "data.data");
                GameObject.FindObjectOfType<EncounterManager>().ShowCardSelectionPanel(encounterSelection);
                break;
            case "select_trinkets":
                var trinkets = FetchData.ParseJsonWithPath<List<Trinket>>(data, "data.data.trinkets");
                GameManager.Instance.EVENT_SHOW_SELECT_TRINKET_PANEL.Invoke(trinkets);
                break;
            case "finish_encounter":
                //  GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
                break;
        }
    }

    private static void ProcessMerchantUpdate(string action, string data)
    {
        switch (action)
        {
            case "begin_merchant":
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Merchant);
                break;
            case "purchase_success":
                GameManager.Instance.EVENT_MERCHANT_PURCHASE_SUCCESS.Invoke(true);
                break;
            case "purchase_failure":
                GameManager.Instance.EVENT_MERCHANT_PURCHASE_SUCCESS.Invoke(false);
                break;
        }
    }

    private static void ProcessCampUpdate(string action, string data)
    {
        switch (action)
        {
            case "begin_camp":
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Camp);
                break;
            case "heal_amount":
                SWSM_HealData healData = JsonConvert.DeserializeObject<SWSM_HealData>(data);
                GameManager.Instance.EVENT_HEAL.Invoke("camp", healData.data.data.healed);
                break;
            case "finish_camp":
                GameManager.Instance.EVENT_CAMP_FINISH.Invoke();
                break;
        }
    }

    private static void ProcessAddPotion(string action, string data)
    {
        //TODO possibly switch effects for different warnings
        switch (action)
        {
            case "potion_not_found_in_database":
            case "potion_not_in_inventory":
            case "potion_max_count_reached":
                GameManager.Instance.EVENT_POTION_WARNING.Invoke(action);
                break;
        }
    }

    private static void ProcessUsePotion(string action, string data)
    {
        switch (action)
        {
            case "potion_not_usable_outside_combat":
                GameManager.Instance.EVENT_POTION_WARNING.Invoke(action);
                break;
        }
    }

    private static void ProcessAddTrinket(string action, string data)
    {
        switch (action)
        {
            case "trinket_not_found_in_database":
                Debug.LogError("Selected trinket not found in database");
                break;
        }
    }
}