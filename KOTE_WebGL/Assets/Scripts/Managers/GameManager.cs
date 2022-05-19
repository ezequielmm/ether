using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;



public class GameManager : SingleTon<GameManager>
{
    //REGISTER ACCOUNT EVENTS
    [HideInInspector]
    public UnityEvent<string, string, string> EVENT_REQUEST_REGISTER = new UnityEvent<string, string, string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_REGISTER_ERROR = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<bool> EVENT_REGISTERPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_NAME = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_NAME_SUCESSFUL = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_NAME_ERROR = new UnityEvent<string>();

    //LOGIN EVENTS
    [HideInInspector]
    public UnityEvent<string, string> EVENT_REQUEST_LOGIN = new UnityEvent<string, string>();
    [HideInInspector]
    public UnityEvent<string, int> EVENT_REQUEST_LOGIN_SUCESSFUL = new UnityEvent<string, int>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_LOGIN_ERROR = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<bool> EVENT_LOGINPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //PROFILE EVENTS
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_PROFILE = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_PROFILE_ERROR = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<ProfileData> EVENT_REQUEST_PROFILE_SUCCESSFUL = new UnityEvent<ProfileData>();
  //  [HideInInspector]
  //  public UnityEvent<PlayerStateData> EVENT_REQUEST_PLAYERSTATE = new UnityEvent<PlayerStateData>();

    //SETTINGS EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_SETTINGSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_LOGOUT = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_LOGOUT_SUCCESSFUL = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_REQUEST_LOGOUT_ERROR = new UnityEvent<string>();

    //WALLETS EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_WALLETSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<bool, GameObject> EVENT_DISCONNECTWALLETPANEL_ACTIVATION_REQUEST = new UnityEvent<bool, GameObject>();
    [HideInInspector]
    public UnityEvent<bool> EVENT_DISCONNECTWALLET_CONFIRMED = new UnityEvent<bool>();

    //TREASURY EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_TREASURYPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //CHARACTER SELECTION EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_CHARACTERSELECTIONPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<string> EVENT_CHARACTERSELECTED = new UnityEvent<string>();

    //REWARDS EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_REWARDSPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<bool> EVENT_CARDS_REWARDPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    //POTIONS EVENTS
    [HideInInspector]
    public UnityEvent<Potion> EVENT_POTION_USED = new UnityEvent<Potion>();

    //ROYAL HOUSE EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_ROYALHOUSES_ACTIVATION_REQUEST = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<ArmoryItem, bool> EVENT_SELECTARMORY_ITEM = new UnityEvent<ArmoryItem, bool>();
    [HideInInspector]
    public UnityEvent<BlessingItem, bool> EVENT_SELECTBLESSING_ITEM = new UnityEvent<BlessingItem, bool>();

    
    //SHOP LOCATION EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_SHOPLOCATION_ACTIVATION_REQUEST = new UnityEvent<bool>();

    
    //EXPEDITION EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_EXPEDITION_STATUS_UPDATE = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent EVENT_EXPEDITION_CONFIRMED = new UnityEvent();
    
    //MAP EVENTS
    [HideInInspector]
    public UnityEvent<string> EVENT_MAP_NODES_UPDATE = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<int> EVENT_MAP_NODE_SELECTED = new UnityEvent<int>();    
    [HideInInspector]
    public UnityEvent<NodeStateData,WS_QUERY_TYPE> EVENT_NODE_DATA_UPDATE = new UnityEvent<NodeStateData, WS_QUERY_TYPE>();
    [HideInInspector]
    public UnityEvent<int> EVENT_MAP_NODE_MOUSE_OVER = new UnityEvent<int>();
    /// <summary>
    /// Scroll map buttons events. First bool enable/disable, second bool direction left/right
    /// </summary>
    [HideInInspector]
    public UnityEvent<bool,bool> EVENT_MAP_SCROLL_CLICK = new UnityEvent<bool,bool>();
    public UnityEvent EVENT_MAP_MASK_DOUBLECLICK = new UnityEvent();

    //MAP PANEL EVENTS
    [HideInInspector]
    public UnityEvent<bool> EVENT_MAP_PANEL_TOOGLE = new UnityEvent<bool>();

    //PLAYER DATA EVENTS
    [HideInInspector]
    public UnityEvent<PlayerStateData> EVENT_PLAYER_STATUS_UPDATE = new UnityEvent<PlayerStateData>();

    //TOP BAR EVENTS
    [HideInInspector]
    public UnityEvent EVENT_MAP_ICON_CLICKED = new UnityEvent();
    [HideInInspector]
    public UnityEvent<bool> EVENT_TOOGLE_TOPBAR_MAP_ICON = new UnityEvent<bool>();
   
    //HAND EVENTS
    [HideInInspector]
    public UnityEvent<PileTypes> EVENT_CARD_PILE_CLICKED = new UnityEvent<PileTypes>();
    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_MOUSE_ENTER = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_MOUSE_EXIT = new UnityEvent<string>();

    //Gameplay events
    [HideInInspector]
    public UnityEvent<string> EVENT_CARD_PLAYED = new UnityEvent<string>();
    [HideInInspector]
    public UnityEvent EVENT_END_TURN_CLICKED = new UnityEvent();
    

    public inGameScenes nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    public WebRequesterManager webRequester;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        nextSceneToLoad = scene;
        SceneManager.LoadScene(inGameScenes.Loader.ToString());
    }
}