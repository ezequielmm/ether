using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : SingleTon<GameManager>
{
    //LOGIN EVENTS
    [HideInInspector] public UnityEvent EVENT_AUTHENTICATED = new UnityEvent();

    //PROFILE EVENTS
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_PROFILE { get; } = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_PROFILE_ERROR = new UnityEvent<string>();

    [HideInInspector]
    public UnityEvent<ProfileData> EVENT_REQUEST_PROFILE_SUCCESSFUL { get; } = new UnityEvent<ProfileData>();

    [HideInInspector] public UnityEvent<string, int> EVENT_UPDATE_NAME_AND_FIEF = new UnityEvent<string, int>();


    //SETTINGS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SETTINGSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT_COMPLETED = new UnityEvent<string>();

    [HideInInspector] public UnityEvent EVENT_VERSION_UPDATED = new();

    //WALLET EVENTS
    // Panel
    [HideInInspector] public UnityEvent<bool> EVENT_WALLETSPANEL_ACTIVATION_REQUEST { get; } = new UnityEvent<bool>();

    //TREASURY EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TREASURYPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //ARMORY EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SHOW_ARMORY_PANEL = new UnityEvent<bool>();

    //CONFIRMATION PANEL EVENTS
    [HideInInspector]
    public UnityEvent<string, Action> EVENT_SHOW_CONFIRMATION_PANEL = new UnityEvent<string, Action>();

    [HideInInspector] public UnityEvent<string, Action, Action> EVENT_SHOW_CONFIRMATION_PANEL_WITH_BACK_ACTION =
        new UnityEvent<string, Action, Action>();

    [HideInInspector]
    public UnityEvent<string, Action, Action, string[]> EVENT_SHOW_CONFIRMATION_PANEL_WITH_FULL_CONTROL =
        new UnityEvent<string, Action, Action, string[]>();

    [HideInInspector] public UnityEvent<string> EVENT_SHOW_WARNING_MESSAGE = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_HIDE_WARNING_MESSAGE = new UnityEvent();

    //CHARACTER SELECTION EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_CHARACTERSELECTED = new UnityEvent<string>();

    //REWARDS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SHOW_REWARDS_PANEL = new UnityEvent<bool>();

    [HideInInspector]
    public UnityEvent<SWSM_RewardsData> EVENT_POPULATE_REWARDS_PANEL = new UnityEvent<SWSM_RewardsData>();

    [HideInInspector] public UnityEvent<string> EVENT_REWARD_SELECTED = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_CONTINUE_EXPEDITION = new UnityEvent();

    //POTIONS EVENTS
    [HideInInspector] public UnityEvent<string, string> EVENT_POTION_USED = new UnityEvent<string, string>();
    [HideInInspector] public UnityEvent<string> EVENT_POTION_DISCARDED = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<PotionManager> EVENT_POTION_SHOW_POTION_MENU = new UnityEvent<PotionManager>();
    [HideInInspector] public UnityEvent<string> EVENT_POTION_WARNING = new UnityEvent<string>();

    //ROYAL HOUSE EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_ROYALHOUSES_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<ArmoryItem, bool> EVENT_SELECTARMORY_ITEM = new UnityEvent<ArmoryItem, bool>();

    [HideInInspector]
    public UnityEvent<BlessingItem, bool> EVENT_SELECTBLESSING_ITEM = new UnityEvent<BlessingItem, bool>();

    //ENCOUNTER EVENTS
    [HideInInspector] public UnityEvent EVENT_SHOW_ENCOUNTER_PANEL = new UnityEvent();

    //MERCHANT EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_MERCHANT_PANEL = new UnityEvent<bool>();

    [HideInInspector]
    public UnityEvent<string, string> EVENT_MERCHANT_BUY = new UnityEvent<string, string>(); // type, id

    [HideInInspector] public UnityEvent<bool> EVENT_MERCHANT_PURCHASE_SUCCESS = new UnityEvent<bool>();

    //CAMP EVENTS
    [HideInInspector] public UnityEvent EVENT_CAMP_SHOW_PANEL = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CAMP_HEAL = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CAMP_FINISH = new UnityEvent();

    //TREASURE EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_TREASURE_ELEMENTS = new UnityEvent<bool>();

    [HideInInspector]
    public UnityEvent<SWSM_TreasureData> EVENT_TREASURE_CHEST_SIZE = new UnityEvent<SWSM_TreasureData>();

    [HideInInspector] public UnityEvent EVENT_TREASURE_OPEN_CHEST = new UnityEvent();

    [HideInInspector]
    public UnityEvent<SWSM_ChestResult> EVENT_TREASURE_CHEST_RESULT = new UnityEvent<SWSM_ChestResult>();

    [HideInInspector] public UnityEvent<int> EVENT_ENCOUNTER_DAMAGE = new UnityEvent<int>();


    //EXPEDITION EVENTS
    [HideInInspector] public UnityEvent EVENT_EXPEDITION_SYNC { get; } = new UnityEvent();

    //MAP EVENTS
    [HideInInspector] public UnityEvent<NodeStateData, WS_QUERY_TYPE> EVENT_NODE_DATA_UPDATE =
        new UnityEvent<NodeStateData, WS_QUERY_TYPE>();

    [HideInInspector] public UnityEvent<SWSM_MapData> EVENT_ALL_MAP_NODES_UPDATE = new UnityEvent<SWSM_MapData>();
    [HideInInspector] public UnityEvent<int> EVENT_MAP_NODE_SELECTED = new UnityEvent<int>();
    [HideInInspector] public UnityEvent<int> EVENT_MAP_NODE_MOUSE_OVER = new UnityEvent<int>();
    [HideInInspector] public UnityEvent<int> OnNodeTransitionEnd = new UnityEvent<int>();


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
    /// <summary>
    /// Toggles the game click blocker.
    /// </summary>
    [HideInInspector] public UnityEvent<bool?> EVENT_TOGGLE_GAME_CLICK = new UnityEvent<bool?>();

    [HideInInspector] public UnityEvent<Trinket> EVENT_TRINKET_ACTIVATED = new UnityEvent<Trinket>();
    //PLAYER DATA EVENTS
    [HideInInspector]
    public UnityEvent<PlayerStateData> EVENT_PLAYER_STATUS_UPDATE { get; } = new UnityEvent<PlayerStateData>();

    [HideInInspector] public UnityEvent<PlayerData> EVENT_UPDATE_PLAYER = new UnityEvent<PlayerData>();

    // NFT SKIN EVENTS
    [HideInInspector] public UnityEvent<Nft> EVENT_NFT_SELECTED { get; } = new UnityEvent<Nft>();
    [HideInInspector] public UnityEvent<Trait, string> EVENT_UPDATE_NFT { get; } = new UnityEvent<Trait, string>();

    [HideInInspector] public UnityEvent EVENT_UPDATE_PLAYER_SKIN { get; } = new UnityEvent();

    //TOP BAR EVENTS
    [HideInInspector] public UnityEvent EVENT_MAP_ICON_CLICKED = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_TOPBAR_MAP_ICON = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<int, int> EVENT_UPDATE_CURRENT_STEP_INFORMATION = new UnityEvent<int, int>();

    //SELECT PANEL EVENTS
    [HideInInspector]
    public UnityEvent<List<Card>, SelectPanelOptions, Action<List<string>>> EVENT_SHOW_SELECT_CARD_PANEL { get; } =
        new UnityEvent<List<Card>, SelectPanelOptions, Action<List<string>>>();

    [HideInInspector] public UnityEvent<Deck> EVENT_CARD_PILE_SHOW_DECK = new UnityEvent<Deck>();

    [HideInInspector]
    public UnityEvent<List<Trinket>> EVENT_SHOW_SELECT_TRINKET_PANEL = new UnityEvent<List<Trinket>>();

    [HideInInspector] public UnityEvent EVENT_HIDE_COMMON_CARD_PANEL = new UnityEvent();
    [HideInInspector] public UnityEvent<List<string>> EVENT_TRINKETS_SELECTED = new UnityEvent<List<string>>();

    //CARDS EVENTS
    [HideInInspector] public UnityEvent<PileTypes> EVENT_CARD_PILE_CLICKED = new UnityEvent<PileTypes>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_ENTER = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string, Vector3> EVENT_CARD_SHOWING_UP = new UnityEvent<string, Vector3>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_EXIT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW_CARDS = new UnityEvent();

    [HideInInspector] public UnityEvent<CardPiles> EVENT_CARDS_PILES_UPDATED = new UnityEvent<CardPiles>();

    //[HideInInspector] public UnityEvent<CardToMoveData, float> EVENT_MOVE_CARD = new UnityEvent<CardToMoveData, float>();
    [HideInInspector]
    public UnityEvent<List<(CardToMoveData, float)>> EVENT_MOVE_CARDS = new UnityEvent<List<(CardToMoveData, float)>>();

    [HideInInspector] public UnityEvent EVENT_REARRANGE_HAND = new UnityEvent();

    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_DISABLED = new UnityEvent<string>(); //id fo the cards being destroyed

    [HideInInspector] public UnityEvent EVENT_CARD_NO_ENERGY = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DISCARD = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_EXHAUST { get; } = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_SHUFFLE = new UnityEvent();
    [HideInInspector] public UnityEvent<AddCardData> EVENT_CARD_ADD = new UnityEvent<AddCardData>();
    [HideInInspector] public UnityEvent<Card> EVENT_CARD_UPDATE_TEXT = new UnityEvent<Card>();

    //Gameplay events
    [HideInInspector]
    public UnityEvent<GameStatuses> EVENT_PREPARE_GAME_STATUS_CHANGE { get; } = new UnityEvent<GameStatuses>();

    [HideInInspector] public UnityEvent<GameStatuses> EVENT_GAME_STATUS_CHANGE = new UnityEvent<GameStatuses>();
    [HideInInspector] public UnityEvent EVENT_GAME_STATUS_CONFIRM = new UnityEvent();

    [HideInInspector]
    public UnityEvent<string, string> EVENT_CARD_PLAYED = new UnityEvent<string, string>(); // cardID, targetID

    [HideInInspector] public UnityEvent EVENT_END_TURN_CLICKED = new UnityEvent();
    [HideInInspector] public UnityEvent<Type, string> EVENT_CONFIRM_EVENT = new UnityEvent<Type, string>();


    //Combat events
    [HideInInspector] public UnityEvent EVENT_START_COMBAT_ENCOUNTER = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_COMBAT_ELEMENTS = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_COMBAT_UI = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent EVENT_FADE_OUT_UI = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_SHOW_PLAYER_CHARACTER = new UnityEvent();

    [HideInInspector]
    public UnityEvent<int, int> EVENT_UPDATE_ENERGY = new UnityEvent<int, int>(); //current energy, max energy 

    [HideInInspector]
    public UnityEvent<int, int> EVENT_UPDATE_PLAYER_HEALTH = new UnityEvent<int, int>(); //current health, max health

    [HideInInspector]
    public UnityEvent<CombatTurnData> EVENT_ATTACK_REQUEST { get; } = new UnityEvent<CombatTurnData>();

    [HideInInspector] public UnityEvent<CombatTurnData> EVENT_ATTACK_RESPONSE = new UnityEvent<CombatTurnData>();
    [HideInInspector] public UnityEvent<CombatTurnData> EVENT_COMBAT_TURN_ENQUEUE = new UnityEvent<CombatTurnData>();
    [HideInInspector] public UnityEvent EVENT_COMBAT_FORCE_CLEAR = new UnityEvent();
    [HideInInspector] public UnityEvent<Guid> EVENT_COMBAT_TURN_END = new UnityEvent<Guid>();
    [HideInInspector] public UnityEvent EVENT_COMBAT_QUEUE_EMPTY = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_COMBAT_ORIGIN_CHANGE = new UnityEvent();
    [HideInInspector] public UnityEvent<StatusData> EVENT_UPDATE_STATUS_EFFECTS = new UnityEvent<StatusData>();
    [HideInInspector] public UnityEvent EVENT_CLEAR_TOOLTIPS = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_TOOLTIPS = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_SHOW_COMBAT_OVERLAY_TEXT = new UnityEvent<string>();

    [HideInInspector] public UnityEvent<string, Action>
        EVENT_SHOW_COMBAT_OVERLAY_TEXT_WITH_ON_COMPLETE = new UnityEvent<string, Action>();

    [HideInInspector]
    public UnityEvent<List<Tooltip>, TooltipController.Anchor, Vector3, Transform>
        EVENT_SET_TOOLTIPS { get; } =
        new UnityEvent<List<Tooltip>, TooltipController.Anchor, Vector3, Transform>();

    [HideInInspector] public UnityEvent<string, int> EVENT_HEAL = new UnityEvent<string, int>(); // id, healed amount

    [HideInInspector] public UnityEvent<CombatTurnData.Target>
        EVENT_DAMAGE = new UnityEvent<CombatTurnData.Target>(); // id, damage amount, break shield

    // pointer events
    [HideInInspector] public UnityEvent<PointerData> EVENT_ACTIVATE_POINTER { get; } = new UnityEvent<PointerData>();
    [HideInInspector] public UnityEvent<string> EVENT_DEACTIVATE_POINTER = new UnityEvent<string>();

    //Common events
    [HideInInspector]
    public UnityEvent<WS_DATA_REQUEST_TYPES> EVENT_GENERIC_WS_DATA { get; } = new UnityEvent<WS_DATA_REQUEST_TYPES>();

    [HideInInspector] public UnityEvent EVENT_WS_CONNECTED = new UnityEvent();
    [HideInInspector] public UnityEvent<string> EVENT_CHANGE_TURN = new UnityEvent<string>();

    //Enemies events
    [HideInInspector] public UnityEvent<EnemiesData> EVENT_UPDATE_ENEMIES { get; } = new UnityEvent<EnemiesData>();
    [HideInInspector] public UnityEvent<EnemiesData> EVENT_ADD_ENEMIES = new UnityEvent<EnemiesData>();
    [HideInInspector] public UnityEvent<EnemyData> EVENT_UPDATE_ENEMY = new UnityEvent<EnemyData>();
    [HideInInspector] public UnityEvent<EnemyIntent> EVENT_UPDATE_INTENT = new UnityEvent<EnemyIntent>();


    // Audio Events
    [HideInInspector] public UnityEvent<SoundTypes, string> EVENT_PLAY_SFX = new UnityEvent<SoundTypes, string>();
    [HideInInspector] public UnityEvent<MusicTypes, int> EVENT_PLAY_MUSIC = new UnityEvent<MusicTypes, int>();
    [HideInInspector] public UnityEvent EVENT_VOLUME_CHANGED = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_STOP_MUSIC = new UnityEvent();


    //Console Events
    [HideInInspector] public UnityEvent EVENT_SHOW_CONSOLE = new UnityEvent();
    [HideInInspector] public UnityEvent<int> EVENT_SKIP_NODE = new UnityEvent<int>();

    // Scene Events
    [HideInInspector] public UnityEvent EVENT_SCENE_LOADING = new UnityEvent();
    [HideInInspector] public UnityEvent<inGameScenes> EVENT_SCENE_LOADED { get; } = new UnityEvent<inGameScenes>();

    // Feedback Reporting Events
    [HideInInspector] public UnityEvent EVENT_SHOW_FEEDBACK_PANEL = new UnityEvent();

    [HideInInspector]
    public UnityEvent<string, string, string> EVENT_SEND_BUG_FEEDBACK = new UnityEvent<string, string, string>();

    [HideInInspector] 
    public UnityEvent<LeaderboardData> EVENT_SHOW_LEADERBOARD = new UnityEvent<LeaderboardData>();
    
    public bool firstLoad = true;

    public inGameScenes
        nextSceneToLoad
    {
        get;
        set;
    } // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    public inGameScenes CurrentScene { get; private set; } = inGameScenes.Loader;
    public static string ServerVersion { get; private set; }
    public PlayerStateData PlayerStateData { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        EnqueueActionForSceneLoad(GetServerVersion, inGameScenes.MainMenu);
        EVENT_REQUEST_LOGOUT_COMPLETED.AddListener(OnLogout);
        EVENT_SCENE_LOADED.AddListener(OnSceneLoad);
        SceneManager.activeSceneChanged += UpdateSoundVolume;
        SceneManager.sceneLoaded += SceneLoaded;
        //EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(ReturnToMainMenu);
    }

    private async void GetServerVersion()
    {
        ServerVersion = await FetchData.Instance.GetServerVersion();
        EVENT_VERSION_UPDATED.Invoke();
    }

    private void SceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        inGameScenes loadedScene = scene.name.ParseToEnum<inGameScenes>();
        if (nextSceneToLoad == loadedScene && CurrentScene == nextSceneToLoad)
        {
            //TODO this needs to be addressed better. This can cause false failures for tests if testing the same function multiple times
            Debug.LogError($"[GameManager] SceneLoaded Called Twice. Not running Event Scene Loaded.");
            return;
        }

        if (nextSceneToLoad == loadedScene)
        {
            CurrentScene = nextSceneToLoad;
            EVENT_SCENE_LOADED.Invoke(nextSceneToLoad);
        }
    }

    public void LoadScene(inGameScenes scene, bool async = false) //Loads the target scene passing through the LoaderScene
    {
        Debug.Log("We are going to :  " + scene + " async " + async);
        EVENT_SCENE_LOADING.Invoke();
        nextSceneToLoad = scene;

        //TODO: This is a hack to fix the issue with the expedition scene not loading properly
        if (scene == inGameScenes.MainMenu) 
            firstLoad = true;
        
        if (scene == inGameScenes.Expedition && CurrentScene != inGameScenes.MainMenu)
        {
            Debug.Log("RequestExpeditionSync");
            RequestExpeditionSync();
        }

        if (scene == inGameScenes.MainMenu)
        {
            EVENT_STOP_MUSIC.Invoke();
        }

        if (async)
            SceneManager.LoadSceneAsync(inGameScenes.Loader.ToString());
        else
            SceneManager.LoadScene(inGameScenes.Loader.ToString());
        CurrentScene = inGameScenes.Loader;
    }

    private void RequestExpeditionSync()
    {
        // Queue a map update for later
        EnqueueActionForSceneLoad(() => { EVENT_EXPEDITION_SYNC.Invoke(); },
            inGameScenes.Expedition);
    }

    private void OnLogout(string message)
    {
        Cleanup.CleanupGame();
    }

    // when the scene changes, update the sound volume using the value stored in PlayerPrefs
    private void UpdateSoundVolume(Scene prevScene, Scene nextScene)
    {
        float volumeSetting = PlayerPrefs.GetFloat("settings_volume");
        AudioListener.volume = volumeSetting;
    }

    List<(Action, inGameScenes)> SceneLoadsActions = new List<(Action, inGameScenes)>();

    public void EnqueueActionForSceneLoad(Action action, inGameScenes sceneName)
    {
        SceneLoadsActions.Add((action, sceneName));
    }

    private void OnSceneLoad(inGameScenes scene)
    {
        Debug.Log($"[GameManager] Scene Loaded: {scene}");
        for (int i = SceneLoadsActions.Count - 1; i >= 0; i--)
        {
            (Action, inGameScenes) action = SceneLoadsActions[i];
            if (action.Item2 == scene)
            {
                action.Item1.Invoke();
                SceneLoadsActions.RemoveAt(i);
            }
        }
    }

    internal void PlayerStateUpdate(PlayerStateData playerStateData)
    {
        this.PlayerStateData = playerStateData;
        EVENT_PLAYER_STATUS_UPDATE.Invoke(playerStateData);
    }
}