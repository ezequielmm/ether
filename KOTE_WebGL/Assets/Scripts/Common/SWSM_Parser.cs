using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWSM_Parser
{ 
    public static void ParseJSON(string data)
    {
        SWSM_Base swsm = JsonUtility.FromJson<SWSM_Base>(data);

        Debug.Log("[MessageType]" + swsm.data.message_type+" , [Action]" +swsm.data.action);

        switch (swsm.data.message_type)
        {
            case "map_update":
                //SWSM_MapData mapData = JsonUtility.FromJson<SWSM_MapData>(data);
                GameManager.Instance.EVENT_MAP_NODES_UPDATE.Invoke(data);
                break;
            case "combat_update":
                //NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
                SWSM_NodeData nodeBase = JsonUtility.FromJson<SWSM_NodeData>(data);
                NodeStateData nodeState = nodeBase.data;
                GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState,WS_QUERY_TYPE.MAP_NODE_SELECTED);                
                break;
            case "enemy_intents":
                ProcessEnemyIntents(swsm.data.action, data);
                break;
            case "player_state_update":
                SWSM_PlayerState playerStateBase = JsonUtility.FromJson<SWSM_PlayerState>(data);
                PlayerStateData playerState = playerStateBase.data;
                GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(playerState);
                break;
            case "error":
                ProcessErrorAction(swsm.data.action, data);
                break;
            default:
                Debug.LogError("No message_type processed. Data Received: " + data);
                break;
        } ;
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
            case "card_unplayable":
                errorData = JsonUtility.FromJson<SWSM_ErrorData>(data);
                Debug.Log(action + ": " + errorData.data);
                break;
            case "invalid_card":
                errorData = JsonUtility.FromJson<SWSM_ErrorData>(data);
                Debug.Log(action + ": " + errorData.data);
                break;
        }
    }



}
