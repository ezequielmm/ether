using System;
using System.Collections.Generic;
using UnityEngine;

public class SWSM_Parser
{
    public static void ParseJSON(string data)
    {
        SWSM_Base swsm = JsonUtility.FromJson<SWSM_Base>(data);

        Debug.Log("[MessageType]" + swsm.data.message_type + " , [Action]" + swsm.data.action);

        switch (swsm.data.message_type)
        {
            case nameof(WS_MESSAGE_TYPES.map_update):
                UpdateMapActionPicker(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.combat_update):               
                ProcessCombatUpdate(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.enemy_intents):
                //ProcessEnemyIntents(swsm.data.action, data);
                Debug.LogWarning($"[SWSM Parser] Enemy Intents are no longer listened for.");
                break;
            case nameof(WS_MESSAGE_TYPES.player_state_update):
                ProcessPlayerStateUpdate( data);
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
            default:
                Debug.LogError("No message_type processed. Data Received: " + data);
                break;
        } ;
    }

    private static void ProcessPlayerStateUpdate(string data)
    {
        SWSM_PlayerState playerStateBase = JsonUtility.FromJson<SWSM_PlayerState>(data);

        PlayerStateData playerState = playerStateBase.data;
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(playerState);
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
        }
    }

    private static void ProcessDrawCards(string data)
    {
        //GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
    }  
    
    private static void ProcessChangeTurn(string data)
    {       

        SWSM_ChangeTurn who = JsonUtility.FromJson<SWSM_ChangeTurn>(data);

        Debug.Log("[ProcessChangeTurn]data= "+data);
        Debug.Log("[ProcessChangeTurn]who.data= " + who.data);
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke(who.data.data);
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
            case nameof(WS_MESSAGE_ACTIONS.create_card):
                ProcessCreateCard(data);
               // ProcessMoveCard(data);
                break;
        }
    }

    private static void ProcessUpdateEnemy(string rawData)
    {

        SWSM_Enemies enemiesData = JsonUtility.FromJson<SWSM_Enemies>(rawData);
        foreach (EnemyData enemyData in enemiesData.data.data)
        {
            GameManager.Instance.EVENT_UPDATE_ENEMY.Invoke(enemyData);
            break;//TODO: process all enemis , not only one
        }
    }

    private static void ProcessMoveCard(string rawData)
    {
        SWSM_CardMove cardMoveData = JsonUtility.FromJson<SWSM_CardMove>(rawData);
        Debug.Log(cardMoveData);
        foreach (CardToMoveData data in cardMoveData.data.data)
        {
            GameManager.Instance.EVENT_MOVE_CARD.Invoke(data);
            //GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
        }

        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
    }

    private static void ProcessCreateCard(string data)
    {
        Debug.Log("[ProcessCreateCard] data:"+data);
        SWSM_CardMove cardMoveData = JsonUtility.FromJson<SWSM_CardMove>(data);
        // GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
       
        foreach (CardToMoveData cardData in cardMoveData.data.data)
        {
            GameManager.Instance.EVENT_CARD_CREATE.Invoke(cardData.id);
        }
    }

    private static void ProcessUpdatePlayer(string data)
    {
        SWSM_Players playersData = JsonUtility.FromJson<SWSM_Players>(data);
       // foreach (PlayerData playerData in playersData.data)
      //  {
            GameManager.Instance.EVENT_UPDATE_PLAYER.Invoke(playersData.data.data);
       // }//TODO: plyersdta will be a list
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
                
                SWSM_CardsPiles deck = JsonUtility.FromJson<SWSM_CardsPiles>(data);
                Debug.Log("Cardspiles ,draw count:" + deck.data.data.draw.Count+" ,hand.count:"+deck.data.data.hand.Count);
                
                GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(deck.data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Enemies):

                // SWSM_Enemies enemies = JsonUtility.FromJson<SWSM_Enemies>(data);
                //   GameManager.Instance.EVENT_UPDATE_ENEMIES.Invoke(enemies.data);
                ProcessUpdateEnemy(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Players):
                ProcessUpdatePlayer(data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.EnemyIntents):
                ProcessEnemyIntents("update_enemy_intents", data);
                break;
            default:
                Debug.Log($"[SWSM Parser] [Generic Data] Uncaught Action \"{action}\". Data = {data}");
                break;
        }
    }

    private static void ProcessCombatUpdate(string action, string data)
    {
        SWSM_NodeData nodeBase = JsonUtility.FromJson<SWSM_NodeData>(data);
        NodeStateData nodeState = nodeBase.data;
        switch (action)
        {
            case "begin_combat":
                
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.Combat);
                break;
        }
    }
    private static void ProcessEnemyIntents(string action, string data)
    {
        Debug.Log($"[ProcessEnemyIntents] data = {data}");
        SWSM_IntentData swsm_intentData = JsonUtility.FromJson<SWSM_IntentData>(data);
        List<EnemyIntent> enemyIntents = swsm_intentData.data.data;
        switch (action) 
        {
            case "update_enemy_intents":
                foreach (EnemyIntent enemyIntent in enemyIntents) {
                    if(enemyIntent != null)
                        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(enemyIntent);
                }
                break;
            default:
                Debug.Log($"[SWSM_Parser] Enemy Intents - {action}: Action not found.");
                break;
        }
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

    private static void UpdateMapActionPicker(string action, string data)
    {
        
        SWSM_MapData mapData = JsonUtility.FromJson<SWSM_MapData>(data);
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
        }
    }

    private static void UpdateEnergy(string data)
    {
        SWSM_EnergyArray energyData = JsonUtility.FromJson<SWSM_EnergyArray>(data);
//        Debug.Log(energyData);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(energyData.data.data[0], energyData.data.data[1]);
    }

}
