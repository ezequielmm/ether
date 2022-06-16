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
            case "player_state_update":
                break;
            case "error":
                ProcessErrorAction(swsm.data.action, data);
                break;
        }
    }

    private static void ProcessErrorAction(string action, string data)
    {
        SWSM_ErrorData errorData;
        //TODO this will need to get parsed and passed on once we know what the error data is going to be
        switch (action)
        {
            case "card_unplayable":
                 errorData = new SWSM_ErrorData
                {
                    data = data
                };
                Debug.Log(errorData.data);
                break;
            case "invalid_card":
                 errorData = new SWSM_ErrorData
                {
                    data = data
                };
                 Debug.Log(errorData.data);
                break;
        }
    }

}
