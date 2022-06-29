using System;
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
                ProcessEnemyIntents(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.player_state_update):
                SWSM_PlayerState playerStateBase = JsonUtility.FromJson<SWSM_PlayerState>(data);

                PlayerStateData playerState = playerStateBase.data;
                GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(playerState);
                break;
            case nameof(WS_MESSAGE_TYPES.error):
                ProcessErrorAction(swsm.data.action, data);
                break;
            //Data types
            case nameof(WS_MESSAGE_TYPES.generic_data):
                ProcessGenericData(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.enemy_attacked):
                ProcessEnemyAttacked(swsm.data.action, data);
                break;
            case nameof(WS_MESSAGE_TYPES.player_attacked):
                ProcessPlayerAttacked(swsm.data.action, data);
                break;
            default:
                Debug.LogError("No message_type processed. Data Received: " + data);
                break;
        } ;
    }

    private static void ProcessEnemyAttacked(string action, string data)
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
            case nameof(WS_MESSAGE_ACTIONS.update_player):break;
        }
    }

    private static void ProcessUpdateEnemy(string rawData)
    {
     
        SWSM_Enemies enemiesData = JsonUtility.FromJson<SWSM_Enemies>(rawData);
        foreach (Enemy enemyData in enemiesData.data.data)
        {
            GameManager.Instance.EVENT_UPDATE_ENEMY.Invoke(enemyData);
        }
    }

    private static void ProcessMoveCard(string rawData)
    {
        SWSM_CardMove cardMoveData = JsonUtility.FromJson<SWSM_CardMove>(rawData);
        Debug.Log(cardMoveData);
        foreach (CardToMoveData data in cardMoveData.data.data)
        {
            GameManager.Instance.EVENT_MOVE_CARD.Invoke(data);
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
        }
    }

    private static void ProcessPlayerAttacked(string action, string data)
    {
        switch (action)
        {
            case nameof(WS_MESSAGE_ACTIONS.update_enemy): break;
            case nameof(WS_MESSAGE_ACTIONS.update_player): break;
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
                SWSM_CardsPiles deck = JsonUtility.FromJson<SWSM_CardsPiles>(data);
                Debug.Log("[OnCardsPilesRequestRespond] deck=" + deck);
                GameManager.Instance.EVENT_CARDS_PILES_UPDATED.Invoke(deck.data);
                break;
            case nameof(WS_DATA_REQUEST_TYPES.Enemies):
                // GameManager.Instance.
                SWSM_Enemies enemies = JsonUtility.FromJson<SWSM_Enemies>(data);
                GameManager.Instance.EVENT_UPDATE_ENEMIES.Invoke(enemies.data);
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
        Debug.Log("[ProcessEnemyIntents]");
       // SWSM_NodeData nodeBase = JsonUtility.FromJson<SWSM_NodeData>(data);
       // NodeStateData nodeState = nodeBase.data;
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
        Debug.Log(energyData);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(energyData.data.data[0], energyData.data.data[1]);
    }

}
