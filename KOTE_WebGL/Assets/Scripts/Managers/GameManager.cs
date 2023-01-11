using System;
using System.Collections.Generic;
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
    
    //SETTINGS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SETTINGSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT_SUCCESSFUL = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_LOGOUT_ERROR = new UnityEvent<string>();

    //WALLET EVENTS
    // Panel
    [HideInInspector] public UnityEvent<bool> EVENT_WALLETSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool, GameObject> EVENT_DISCONNECT_WALLET_PANEL_ACTIVATION_REQUEST =
        new UnityEvent<bool, GameObject>();
    // data requests
    [HideInInspector] public UnityEvent<string> EVENT_WALLET_ADDRESS_RECEIVED = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string> EVENT_REQUEST_WALLET_CONTENTS = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<WalletKnightIds> EVENT_WALLET_CONTENTS_RECEIVED = new UnityEvent<WalletKnightIds>();
    [HideInInspector] public UnityEvent<string> EVENT_MESSAGE_SIGN = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<bool> EVENT_DISCONNECT_WALLET_CONFIRMED = new UnityEvent<bool>();

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

    [HideInInspector] public UnityEvent<string> EVENT_SHOW_WARNING_MESSAGE = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_HIDE_WARNING_MESSAGE = new UnityEvent();

    //CHARACTER SELECTION EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_CHARACTERSELECTED = new UnityEvent<string>();

    //REWARDS EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_SHOW_REWARDS_PANEL = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<SWSM_RewardsData> EVENT_POPULATE_REWARDS_PANEL = new UnityEvent<SWSM_RewardsData>();
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
    [HideInInspector] public UnityEvent<SWSM_EncounterData> EVENT_POPULATE_ENCOUNTER_PANEL = new UnityEvent<SWSM_EncounterData>();
    [HideInInspector] public UnityEvent<int> EVENT_ENCOUNTER_OPTION_SELECTED = new UnityEvent<int>();

    //MERCHANT EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_MERCHANT_PANEL = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<MerchantData> EVENT_POPULATE_MERCHANT_PANEL = new UnityEvent<MerchantData>();
    [HideInInspector] public UnityEvent<string, string> EVENT_MERCHANT_BUY = new UnityEvent<string, string>(); // type, id
    [HideInInspector] public UnityEvent<bool> EVENT_MERCHANT_PURCHASE_SUCCESS = new UnityEvent<bool>();

    //CAMP EVENTS
    [HideInInspector] public UnityEvent EVENT_CAMP_SHOW_PANEL = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CAMP_HEAL = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CAMP_FINISH = new UnityEvent();
    
    //TREASURE EVENTS
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_TREASURE_ELEMENTS = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<SWSM_TreasureData> EVENT_TREASURE_CHEST_SIZE = new UnityEvent<SWSM_TreasureData>();
    [HideInInspector] public UnityEvent EVENT_TREASURE_OPEN_CHEST = new UnityEvent();
    [HideInInspector] public UnityEvent<SWSM_ChestResult> EVENT_TREASURE_CHEST_RESULT = new UnityEvent<SWSM_ChestResult>();
    [HideInInspector] public UnityEvent<int> EVENT_ENCOUNTER_DAMAGE = new UnityEvent<int>();


    //UPGRADE CARDS EVENTS
    [HideInInspector] public UnityEvent<Deck> EVENT_SHOW_UPGRADE_CARDS_PANEL = new UnityEvent<Deck>(); //event from the BE to show the upgradable cards panel
    [HideInInspector] public UnityEvent<string> EVENT_GET_UPGRADE_PAIR = new UnityEvent<string>(); // when the user click a card, we need the show the upgraded card data. We send
    [HideInInspector] public UnityEvent<Deck> EVENT_SHOW_UPGRADE_PAIR = new UnityEvent<Deck>();// BE sending us the 2 cards
    [HideInInspector] public UnityEvent<string> EVENT_USER_CONFIRMATION_UPGRADE_CARD = new UnityEvent<string>();// the user confirmed to upgrade this card id. We send
    [HideInInspector] public UnityEvent<SWSM_ConfirmUpgrade> EVENT_UPGRADE_CONFIRMED = new UnityEvent<SWSM_ConfirmUpgrade>();//from BE confirming. Can contain an error    
   

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
    /// <summary>
    /// Toggles the game click blocker.
    /// </summary>
    [HideInInspector] public UnityEvent<bool?> EVENT_TOGGLE_GAME_CLICK = new UnityEvent<bool?>();

    //PLAYER DATA EVENTS
    [HideInInspector] public UnityEvent<PlayerStateData> EVENT_PLAYER_STATUS_UPDATE = new UnityEvent<PlayerStateData>();
    [HideInInspector] public UnityEvent<PlayerData> EVENT_UPDATE_PLAYER = new UnityEvent<PlayerData>();
    
    // NFT SKIN EVENTS
    [HideInInspector] public UnityEvent<int[]> EVENT_REQUEST_NFT_METADATA = new UnityEvent<int[]>();
    [HideInInspector] public UnityEvent<NftData> EVENT_NFT_METADATA_RECEIVED = new UnityEvent<NftData>();
    [HideInInspector] public UnityEvent<string, string> EVENT_REQUEST_NFT_IMAGE = new UnityEvent<string, string>();
    [HideInInspector] public UnityEvent<string, Sprite> EVENT_NFT_IMAGE_RECEIVED = new UnityEvent<string, Sprite>();
    [HideInInspector] public UnityEvent<TraitSprite> EVENT_REQUEST_NFT_SKIN_SPRITE = new UnityEvent<TraitSprite>();
    [HideInInspector] public UnityEvent<TraitSprite> EVENT_NFT_SKIN_SPRITE_RECEIVED = new UnityEvent<TraitSprite>();
    [HideInInspector] public UnityEvent EVENT_NFT_SKIN_SPRITE_FAILED = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_UPDATE_PLAYER_SKIN = new UnityEvent();
    
    //TOP BAR EVENTS
    [HideInInspector] public UnityEvent EVENT_MAP_ICON_CLICKED = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_TOPBAR_MAP_ICON = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<int, int> EVENT_UPDATE_CURRENT_STEP_INFORMATION = new UnityEvent<int, int>();

    //CARD PANEL EVENTS
    [HideInInspector] public UnityEvent<List<Card>, SelectPanelOptions, Action<List<string>>> EVENT_SHOW_SELECT_CARD_PANEL = new UnityEvent<List<Card>, SelectPanelOptions, Action<List<string>>>();
    [HideInInspector] public UnityEvent<List<Card>, SelectPanelOptions, Action<string>> EVENT_SHOW_DIRECT_SELECT_CARD_PANEL = new UnityEvent<List<Card>, SelectPanelOptions, Action<string>>();
    [HideInInspector] public UnityEvent<Deck> EVENT_CARD_PILE_SHOW_DECK = new UnityEvent<Deck>();
        [HideInInspector] public UnityEvent<List<string>> EVENT_CARDS_SELECTED = new UnityEvent<List<string>>();
    [HideInInspector] public UnityEvent EVENT_HIDE_COMMON_CARD_PANEL = new UnityEvent();

    //CARDS EVENTS
    [HideInInspector] public UnityEvent<PileTypes> EVENT_CARD_PILE_CLICKED = new UnityEvent<PileTypes>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_ENTER = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string, Vector3> EVENT_CARD_SHOWING_UP = new UnityEvent<string, Vector3>();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_MOUSE_EXIT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW_CARDS = new UnityEvent();
    [HideInInspector] public UnityEvent<CardPiles> EVENT_CARDS_PILES_UPDATED = new UnityEvent<CardPiles>();
    [HideInInspector] public UnityEvent<CardToMoveData, float> EVENT_MOVE_CARD = new UnityEvent<CardToMoveData, float>();
    [HideInInspector] public UnityEvent EVENT_REARRANGE_HAND = new UnityEvent();

    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_DISABLED = new UnityEvent<string>(); //id fo the cards being destroyed

    [HideInInspector] public UnityEvent EVENT_CARD_NO_ENERGY = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DRAW = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_DISCARD = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_EXHAUST { get; } = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_CARD_SHUFFLE = new UnityEvent();
    [HideInInspector] public UnityEvent<string> EVENT_CARD_CREATE = new UnityEvent<string>();

    //Gameplay events
    [HideInInspector] public UnityEvent<GameStatuses> EVENT_PREPARE_GAME_STATUS_CHANGE = new UnityEvent<GameStatuses>();
    [HideInInspector] public UnityEvent<GameStatuses> EVENT_GAME_STATUS_CHANGE = new UnityEvent<GameStatuses>();
    [HideInInspector] public UnityEvent<string, string> EVENT_CARD_PLAYED = new UnityEvent<string, string>(); // cardID, targetID
    [HideInInspector] public UnityEvent EVENT_END_TURN_CLICKED = new UnityEvent();
    [HideInInspector] public UnityEvent<Type, string> EVENT_CONFIRM_EVENT = new UnityEvent<Type, string>();


    //Combat events
    [HideInInspector] public UnityEvent EVENT_START_COMBAT_ENCOUNTER = new UnityEvent();
    [HideInInspector] public UnityEvent<bool> EVENT_TOOGLE_COMBAT_ELEMENTS = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent EVENT_TOGGLE_COMBAT_UI = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_SHOW_PLAYER_CHARACTER = new UnityEvent();
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
    [HideInInspector] public UnityEvent<bool> EVENT_TOGGLE_TOOLTIPS = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<string> EVENT_SHOW_COMBAT_OVERLAY_TEXT = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string, Action> EVENT_SHOW_COMBAT_OVERLAY_TEXT_WITH_ON_COMPLETE = new UnityEvent<string, Action>();
    [HideInInspector] public UnityEvent<System.Collections.Generic.List<Tooltip>, TooltipController.Anchor, Vector3, Transform>  EVENT_SET_TOOLTIPS { get; } = 
        new UnityEvent<System.Collections.Generic.List<Tooltip>, TooltipController.Anchor, Vector3, Transform>();
    [HideInInspector] public UnityEvent<string, int> EVENT_HEAL = new UnityEvent<string, int>(); // id, healed amount
    [HideInInspector] public UnityEvent<CombatTurnData.Target> EVENT_DAMAGE = new UnityEvent<CombatTurnData.Target>(); // id, damage amount, break shield

    // pointer events
    [HideInInspector] public UnityEvent<PointerData> EVENT_ACTIVATE_POINTER { get; } = new UnityEvent<PointerData>();
    [HideInInspector] public UnityEvent<string> EVENT_DEACTIVATE_POINTER = new UnityEvent<string>();

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
    [HideInInspector] public UnityEvent<SoundTypes, string> EVENT_PLAY_SFX = new UnityEvent<SoundTypes, string>();
    [HideInInspector] public UnityEvent<MusicTypes, int> EVENT_PLAY_MUSIC = new UnityEvent<MusicTypes, int>();
    [HideInInspector] public UnityEvent EVENT_VOLUME_CHANGED = new UnityEvent();
    [HideInInspector] public UnityEvent EVENT_STOP_MUSIC = new UnityEvent();


    //Console Events
    [HideInInspector] public UnityEvent EVENT_SHOW_CONSOLE = new UnityEvent();
    [HideInInspector] public UnityEvent<int> EVENT_SKIP_NODE = new UnityEvent<int>();

   

    public inGameScenes
        nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    public WebRequesterManager webRequester;


    // Start is called before the first frame update
    void Start()
    {
        EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(OnLogout);
        EVENT_REQUEST_LOGOUT_ERROR.AddListener(OnLogout);
        SceneManager.activeSceneChanged += UpdateSoundVolume;
        //EVENT_REQUEST_LOGOUT_SUCCESSFUL.AddListener(ReturnToMainMenu);
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        nextSceneToLoad = scene;
        if (scene == inGameScenes.MainMenu)
        {
            EVENT_STOP_MUSIC.Invoke();
        }
        SceneManager.LoadScene(inGameScenes.Loader.ToString());
    }

    private void OnLogout(string message)
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