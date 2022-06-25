using UnityEngine;

public class SWSM_Parser
{
    public static void ParseJSON(string data)
    {
        SWSM_Base swsm = JsonUtility.FromJson<SWSM_Base>(data);

        Debug.Log("[MessageType]" + swsm.data.message_type + " , [Action]" + swsm.data.action);

        switch (swsm.data.message_type)
        {
            case "map_update":
                UpdateMapActionPicker(swsm.data.action, data);
                break;
            case "combat_update":
                //NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
               
                //GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState, WS_QUERY_TYPE.MAP_NODE_SELECTED);
                ProcessCombatUpdate(swsm.data.action, data);
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

    // we don't need the SWSM_base here, because we just need the action.
    // We parse the rest of the data from the original message string, and don't retain the message type or action
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

}
