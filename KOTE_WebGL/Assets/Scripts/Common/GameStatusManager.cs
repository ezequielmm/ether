using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatusManager : MonoBehaviour
{
    public GameStatuses currentGameStatus;
    public NodeStateData lastNodeStatusData;

    private PlayerStateData playerStateData;

    bool gameAboutToEnd;


    void Start()
    {
        gameAboutToEnd = false;
        //TODO: for the moment we always start as map but this could change
        OnChangeGameStatus(GameStatuses.Map);

        GameManager.Instance.EVENT_NODE_DATA_UPDATE
            .AddListener(OnNodeDataUpdate); //a node has been selected an we got an update node data

        // GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStatusUpdate);
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnChangeGameStatus);
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener(OnPrepareStatusChange);
        GameManager.Instance.EVENT_CONFIRM_EVENT.AddListener(OnEventConfirmation);
    }

    private void OnPlayerStatusUpdate(PlayerStateData playerState)
    {
        Debug.Log(playerState);
    }

    private void OnNodeDataUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        lastNodeStatusData = nodeState;
        //TODO: for the moment is only combat but as long as we create more node types that logic will be decided here
        if (wsType == WS_QUERY_TYPE.MAP_NODE_SELECTED) OnChangeGameStatus(GameStatuses.Combat);
    }

    public void OnPrepareStatusChange(GameStatuses newGameStatus)
    {
        switch (newGameStatus)
        {
            case GameStatuses.Combat: break;
            case GameStatuses.Map: break;
            case GameStatuses.Encounter: break;
            case GameStatuses.Merchant: break;
            case GameStatuses.RoyalHouse: break;
            case GameStatuses.GameOver:
                gameAboutToEnd = true;
                break;
        }
    }

    public void OnEventConfirmation(string eventName) 
    {
        switch (eventName) 
        {
            case nameof(PlayerState.dying):
                if (gameAboutToEnd)
                {
                    // Prevent Game-Level Interaction (UI Only)
                    GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
                }
                break;
            case nameof(PlayerState.dead):
                if (gameAboutToEnd)
                {
                    // End game when last player dies
                    GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(GameStatuses.GameOver);
                }
                break;
        }
    }

    public void OnChangeGameStatus(GameStatuses newGameStatus)
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
            case GameStatuses.Treasure:
                InitializeTreasureMode();
                break;
            case GameStatuses.Encounter:
                InitializeEncounterNode();
                break;
            case GameStatuses.Merchant:
                InitializeMerchantNode();
                break;
            case GameStatuses.Camp:
                InitializeCampNode();
                break;
            case GameStatuses.RoyalHouse: break;
            case GameStatuses.GameOver:
                gameAboutToEnd = false;
                break;
        }
    }

    private void InitializeMapMode()
    {
        Debug.Log("[InitializeMapMode]");
        //generate map using nodes data. This is currently done on the websocket manager
        //tell top bar to hide the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(false);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(true);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(false);
    }

    private void InitializeCombatmode()
    {
        //tell top bar to show the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(true);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(false);
        //GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.CardsPiles);
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
        //Invoke("InvokeDrawCards", 0.2f);
    }

    private void InitializeTreasureMode()
    {
        //tell top bar to show the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(true);
    }

    private void InitializeEncounterNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.Invoke();
    }

    private void InitializeMerchantNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_SHOW_MERCHANT_PANEL.Invoke();
    }

    private void InitializeCampNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
    }

    private void InitializeRoyaHouseNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    private void InvokeDrawCards()
    {
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
    }
}