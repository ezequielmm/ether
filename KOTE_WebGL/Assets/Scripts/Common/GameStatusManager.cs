using System;
using UnityEngine;

public class GameStatusManager : MonoBehaviour
{
    public GameStatuses currentGameStatus;
    public NodeStateData lastNodeStatusData;

    private PlayerStateData playerStateData;

    GameStatuses preppingStatus;
    private float preppingSince;


    void Start()
    {
        preppingStatus = GameStatuses.None;
        //TODO: for the moment we always start as map but this could change
        OnChangeGameStatus(GameStatuses.Map);

        GameManager.Instance.EVENT_NODE_DATA_UPDATE
            .AddListener(OnNodeDataUpdate); //a node has been selected an we got an update node data

        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnChangeGameStatus);
        GameManager.Instance.EVENT_PREPARE_GAME_STATUS_CHANGE.AddListener(OnPrepareStatusChange);
        GameManager.Instance.EVENT_CONFIRM_EVENT.AddListener(OnEventConfirmation);
        GameManager.Instance.EVENT_GAME_STATUS_CONFIRM.AddListener(CheckForHangingStatuses);
        GameManager.Instance.EVENT_COMBAT_QUEUE_EMPTY.AddListener(CheckForHangingStatuses);
    }

    private void Update()
    {
        if (preppingStatus != GameStatuses.None && Time.time - preppingSince > GameSettings.END_WAIT_TIMEOUT)
        {
            CheckForHangingStatuses();
        }
    }

    private void OnNodeDataUpdate(NodeStateData nodeState, WS_QUERY_TYPE wsType)
    {
        lastNodeStatusData = nodeState;
        //TODO: for the moment is only combat but as long as we create more node types that logic will be decided here
        if (wsType == WS_QUERY_TYPE.MAP_NODE_SELECTED) OnChangeGameStatus(GameStatuses.Combat);
    }

    public void OnPrepareStatusChange(GameStatuses newGameStatus)
    {
        preppingStatus = newGameStatus;
        preppingSince = Time.time;
    }

    public void CheckForHangingStatuses()
    {
        if (preppingStatus == GameStatuses.None) return;
        
        Debug.LogWarning($"[GameStatusManager] Hanging Status ({preppingStatus}) has not been taken care of. Resolving now...");
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(preppingStatus);
    }

    public void OnEventConfirmation(Type enumType, string eventName)
    {
        if (preppingStatus is not (GameStatuses.GameOver or GameStatuses.RewardsPanel))
        {
            return;
        }

        switch (eventName)
        {
            case nameof(PlayerState.dying) or nameof(EnemyState.dying):
                // Prevent Game-Level Interaction (UI Only)
                GameManager.Instance.EVENT_TOGGLE_GAME_CLICK.Invoke(true);
                break;
            case nameof(PlayerState.dead) or nameof(EnemyState.dead):
                // Fulfils prepping status
                GameManager.Instance.EVENT_GAME_STATUS_CHANGE.Invoke(preppingStatus);
                break;
        }
    }

    public void OnChangeGameStatus(GameStatuses newGameStatus)
    {
        currentGameStatus = newGameStatus;
        preppingStatus = GameStatuses.None;
        switch (newGameStatus)
        {
            case GameStatuses.Combat:
                InitializeCombatMode();
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
            case GameStatuses.RewardsPanel:
                InitializeRewards();
                break;
            case GameStatuses.GameOver:
                ScoreboardManager.Instance.UpdateAndShow();
                break;
            case GameStatuses.RoyalHouse:
            default:
                Debug.LogWarning("[GameStatusManager] This game status is not implemented!");
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
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(false);
    }

    private void InitializeCombatMode()
    {
        //tell top bar to show the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(true);
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Battle Start");
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(false);
        Invoke("InvokeDrawCards", 0.2f);
    }

    private void InitializeTreasureMode()
    {
        //tell top bar to show the map icon
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_SHOW_PLAYER_CHARACTER.Invoke();
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.Invoke(true);
    }

    private void InitializeEncounterNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_SHOW_ENCOUNTER_PANEL.Invoke();
    }

    private void InitializeMerchantNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_MERCHANT_PANEL.Invoke(true);
    }

    private void InitializeCampNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
        GameManager.Instance.EVENT_CAMP_SHOW_PANEL.Invoke();
    }

    private void InitializeRoyaHouseNode()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_TOGGLE_COMBAT_ELEMENTS.Invoke(false);
    }

    private void InitializeRewards()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        GameManager.Instance.EVENT_MAP_PANEL_TOGGLE.Invoke(false);
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Rewards);
    }

    private void InvokeDrawCards()
    {
        GameManager.Instance.EVENT_CARD_DRAW_CARDS.Invoke();
    }
}