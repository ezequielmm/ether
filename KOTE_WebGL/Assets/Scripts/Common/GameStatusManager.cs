using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatusManager : MonoBehaviour
{
    public GameStatuses currentGameStatus;

    private PlayerStateData playerStateData;

    void Start()
    {
        //TODO: for the moment we always start as map but this could change
        ChangeGameStatus(GameStatuses.Map);

        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdate);//a node has been selected an we got an update node data

        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);
    }

    private void OnPlayerStatusUpdate(PlayerStateData playerState)
    {        
        Debug.Log(playerState);
    }

    private void OnNodeDataUpdate(NodeStateData nodeState, bool initialCall)
    {
        //TODO: for the moment is only combat but as long as we create more node types that logic will be decided here
        if(initialCall) ChangeGameStatus(GameStatuses.Combat);
    }

    public void ChangeGameStatus(GameStatuses newGameStatus)
    {
        currentGameStatus = newGameStatus;

        switch (newGameStatus)
        {
            case GameStatuses.Combat:
                InitializeCombatmode();
                break;
            case GameStatuses.Map:
                InitializeMapMode();
                break;
            case GameStatuses.Encounter:break;
            case GameStatuses.Merchant:break;
            case GameStatuses.RoyalHouse:break;
        }
    }

    private void InitializeMapMode()
    {
        Debug.Log("[InitializeMapMode]");
        //generate map using nodes data. This is currently done on the websocket manager
        //tell top bar to hide the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(false);
    }

    private void InitializeCombatmode()
    {
        //tell top bar to hide the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
    }
}
