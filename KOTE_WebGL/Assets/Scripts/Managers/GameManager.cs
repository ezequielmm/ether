using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class GameManager : SingleTon<GameManager>
{
    //REGISTER ACCOUNT EVENTS
    [HideInInspector]
    public UnityEvent<string, string, string> EVENT_REQUEST_REGISTER = new UnityEvent<string, string, string>();

    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_REGISTER_ERROR = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<bool> EVENT_REGISTERPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_NAME = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_NAME_SUCESSFUL = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_NAME_ERROR = new UnityEvent<string>();


    //LOGIN EVENTS
    [HideInInspector] public UnityEvent<string, string> EVENT_REQUEST_LOGIN = new UnityEvent<string, string>();
    [HideInInspector] public UnityEvent<string, int> EVENT_REQUEST_LOGIN_SUCESSFUL = new UnityEvent<string, int>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGIN_ERROR = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<bool> EVENT_LOGINPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //PROFILE EVENTS
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_PROFILE = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_PROFILE_ERROR = new UnityEvent<string>();

    [HideInInspector] public UnityEvent<ProfileData> EVENT_REQUEST_PROFILE_SUCCESSFUL = new UnityEvent<ProfileData>();
    //  [HideInInspector]
    //  public UnityEvent<PlayerStateData> EVENT_REQUEST_PLAYERSTATE = new UnityEvent<PlayerStateData>();

    //SETTINGS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SETTINGSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT_SUCCESSFUL = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT_ERROR = new UnityEvent<string>();

    //WALLETS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_WALLETSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    [HideInInspector] public UnityEvent<bool, GameObject> EVENT_DISCONNECTWALLETPANEL_ACTIVATION_REQUEST =
        new UnityEvent<bool, GameObject>();

    [HideInInspector] public UnityEvent<bool> EVENT_DISCONNECTWALLET_CONFIRMED = new UnityEvent<bool>();

    //TREASURY EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TREASURYPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //ARMORY EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_ARMORYPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //CONFIRMATION PANEL EVENTS
    [HideInInspector]
    public UnityEvent<string, Action> EVENT_SHOW_CONFIRMATION_PANEL = new UnityEvent<string, Action>();

    [HideInInspector] public UnityEvent<string, Action, Action> EVENT_SHOW_CONFIRMATION_PANEL_WITH_BACK_ACTION =
        new UnityEvent<string, Action, Action>();

    [HideInInspector]
    public UnityEvent<string, Action, Action, string[]> EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL =
        new UnityEvent<string, Action, Action, string[]>();

    //CHARACTER SELECTION EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_CHARACTERSELECTED = new UnityEvent<string>();

    //REWARDS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_REWARDSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //POTIONS EVENTS
    [HideInInspector] public UnityEvent<Potion> EVENT_POTION_USED = new UnityEvent<Potion>();

    //ROYAL HOUSE EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_ROYALHOUSES_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<ArmoryItem, bool> EVENT_SELECTARMORY_ITEM = new UnityEvent<ArmoryItem, bool>();

    [HideInInspector]
    public UnityEvent<BlessingItem, bool> EVENT_SELECTBLESSING_ITEM = new UnityEvent<BlessingItem, bool>();


    //SHOP LOCATION EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SHOPLOCATION_ACTIVATION_REQUEST = new UnityEvent<bool>();


    //EXPEDITION EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_EXPEDITION_STATUS_UPDATE = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent EVENT_EXPEDITION_CONFIRMED = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_REQUEST_EXPEDITION_CANCEL = new UnityEvent();

    //MAP EVENTS
    [HideInInspector]
    public UnityEvent<NodeStateData, WS_QUERY_TYPE> EVENT_NODE_DATA_UPDATE =
        new UnityEvent<NodeStateData, WS_QUERY_TYPE>();

    [HideInInspector] public UnityEvent<SWSM_MapData> EVENT_ALL_MAP_NODES_UPDATE = new UnityEvent<SWSM_MapData>();
    [HideInInspector] public UnityEvent<int> EVENT_MAP_NODE_SELECTED = new UnityEvent<int>();
    [HideInInspector] public UnityEvent<int> EVENT_MAP_NODE_MOUSE_OVER = new UnityEvent<int>();


    // map animation events
    [HideInInspector] public UnityEvent<SWSM_MapData> EVENT_MAP_REVEAL = new UnityEvent<SWSM_MapData>();
    [HideInInspector] public UnityEvent<int, int> EVENT_MAP_ANIMATE_STEP = new UnityEvent<int, int>();

    [HideInInspector] public UnityEvent<SWSM_MapData> EVENT_MAP_ACTIVATE_PORTAL = new UnityEvent<SWSM_MapData>();
    //[HideInInspector]
    //public UnityEvent<bool> EVENT_MAP_TOGGLE_MAP = new UnityEvent<bool>();

    /// <summary>
    /// Scroll map buttons events. First bool enable/disable, second bool direction left/right
    /// </summary>
    [HideInInspector] public UnityEvent<bool, bool> EVENT_MAP_SCROLL_CLICK = new UnityEvent<bool, bool>();
    [HideInInspector] public UnityEvent<Vector3> EVENT_MAP_SCROLL_DRAG = new UnityEvent<Vector3>();
    [HideInInspector] public UnityEvent EVENT_MAP_MASK_DOUBLECLICK = new UnityEvent();

    //MAP PANEL EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_MAP_PANEL_TOOGLE = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> EVENT_MAP_PANEL_TOGGLE = new UnityEvent<bool>();

    // UI Events
    [HideInInspector] public UnityEvent<bool?> EVENT_TOGGLE_GAME_CLICK = new UnityEvent<bool?>();

    //PLAYER DATA EVENTS
    [HideInInspector] public UnityEvent<PlayerStateData> EVENT_PLAYER_STATUS_UPDATE = new UnityEvent<PlayerStateData>();
    [HideInInspector] public UnityEvent<PlayerData> EVENT_UPDATE_PLAYER = new UnityEvent<PlayerData>();

    //TOP BAR EVENTS
    [HideInInspector] public UnityEvent EVENT_MAP_ICON_CLICKED = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_TOPBAR_MAP_ICON = new UnityEvent<bool>();

    //CARDS EVENTS
    [HideInInspector] public UnityEvent<PileTypes> EVENT_CARD_PILE_CLICKED = new UnityEvent<PileTypes>();
    [HideInInspector] public UnityEvent<Deck> EVENT_CARD_PILE_SHOW_DECK = new UnityEvent<Deck>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_ENTER = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string, Vector3> EVENT_CARD_SHOWING_UP = new UnityEvent<string, Vector3>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_EXIT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<Vector3> EVENT_CARD_ACTIVATE_POINTER = new UnityEvent<Vector3>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_DEACTIVATE_POINTER = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW_CARDS = new UnityEvent();
    [HideInInspector] public UnityEvent<CardPiles> EVENT_CARDS_PILES_UPDATED = new UnityEvent<CardPiles>();
    [HideInInspector] public UnityEvent<CardToMoveData, float> EVENT_MOVE_CARD = new UnityEvent<CardToMoveData, float>();

    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_DISABLED { get; } = new UnityEvent<string>(); //id fo the cards being destroyed

    [HideInInspector] public UnityEvent EVENT_CARD_NO_ENERGY = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DISCARD = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_EXHAUST = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_SHUFFLE = new UnityEvent();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_CREATE = new UnityEvent<string>();

    //Gameplay events
    [HideInInspector] public UnityEvent<GameStatuses> EVENT_GAME_STATUS_CHANGE = new UnityEvent<GameStatuses>();
    [HideInInspector] public UnityEvent<string, int> EVENT_CARD_PLAYED = new UnityEvent<string, int>();
    [HideInInspector] public UnityEvent EVENT_END_TURN_CLICKED = new UnityEvent();

    //Combat events
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_COMBAT_ELEMENTS = new UnityEvent<bool>();   
    [HideInInspector] public UnityEvent<int, int> EVENT_UPDATE_ENERGY = new UnityEvent<int, int>();//current energy, max energy 
    [HideInInspector] public UnityEvent<int, int> EVENT_UPDATE_PLAYER_HEALTH = new UnityEvent<int, int>();//current health, max health
    [HideInInspector] public UnityEvent<CombatTurnData> EVENT_ATTACK_REQUEST = new UnityEvent<CombatTurnData>();
    [HideInInspector] public UnityEvent<CombatTurnData> EVENT_ATTACK_RESPONSE = new UnityEvent<CombatTurnData>();
    [HideInInspector] public UnityEvent<CombatTurnData> EVENT_COMBAT_TURN_ENQUEUE = new UnityEvent<CombatTurnData>();
    [HideInInspector] public UnityEvent<Guid> EVENT_COMBAT_TURN_END = new UnityEvent<Guid>();
    [HideInInspector] public UnityEvent EVENT_CLEAR_COMBAT_QUEUE = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_COMBAT_QUEUE_EMPTY = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_COMBAT_ORIGIN_CHANGE = new UnityEvent();
    [HideInInspector] public UnityEvent<StatusData> EVENT_UPDATE_STATUS_EFFECTS = new UnityEvent<StatusData>();
    [HideInInspector] public UnityEvent EVENT_CLEAR_TOOLTIPS = new UnityEvent();
    [HideInInspector] public UnityEvent<System.Collections.Generic.List<Tooltip>, TooltipController.Anchor, Vector3, Transform>  EVENT_SET_TOOLTIPS { get; } = 
        new UnityEvent<System.Collections.Generic.List<Tooltip>, TooltipController.Anchor, Vector3, Transform>();


    //Common events
    [HideInInspector]
    public UnityEvent<WS_DATA_REQUEST_TYPES> EVENT_GENERIC_WS_DATA = new UnityEvent<WS_DATA_REQUEST_TYPES>();

    [HideInInspector] public UnityEvent EVENT_WS_CONNECTED = new UnityEvent();
    [HideInInspector] public UnityEvent<string> EVENT_CHANGE_TURN = new UnityEvent<string>();

    //Enemies events
    [HideInInspector] public UnityEvent<EnemiesData> EVENT_UPDATE_ENEMIES = new UnityEvent<EnemiesData>();
    [HideInInspector] public UnityEvent<EnemyData> EVENT_UPDATE_ENEMY = new UnityEvent<EnemyData>();
    [HideInInspector] public UnityEvent<EnemyIntent> EVENT_UPDATE_INTENT = new UnityEvent<EnemyIntent>();


    // Audio Events
    [HideInInspector] public UnityEvent<string> EVENT_PLAY_SFX { get; } = new UnityEvent<string>();

    public inGameScenes
        nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    public WebRequesterManager webRequester;


    // Start is called before the first frame update
    void Start()
    {
        EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogoutSuccessful);
        SceneManager.activeSceneChanged += UpdateSoundVolume;
        //EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(ReturnToMainMenu);
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        nextSceneToLoad = scene;
        SceneManager.LoadScene(inGameScenes.Loader.ToString());
    }

    private void OnLogoutSuccessful(string message)
    {
        LoadScene(inGameScenes.MainMenu);
    }

    // when the scene changes, update the sound volume using the value stored in PlayerPrefs
    private void UpdateSoundVolume(Scene prevScene, Scene nextScene)
    {
        float volumeSetting = PlayerPrefs.GetFloat("settings_volume");
        AudioListener.volume = volumeSetting;
    }
}