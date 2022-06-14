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
                UpdateMapActionPicker(swsm.data.action, data);
                break;
            case "combat_update":
                //NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
                SWSM_NodeData nodeBase = JsonUtility.FromJson<SWSM_NodeData>(data);
                NodeStateData nodeState = nodeBase.data;
                GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState,WS_QUERY_TYPE.MAP_NODE_SELECTED);                
                break;
            case "player_state_update":
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
