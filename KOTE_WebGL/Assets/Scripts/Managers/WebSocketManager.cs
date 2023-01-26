using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using System.Text;

public class WebSocketManager : SingleTon<WebSocketManager>
{
    SocketManager manager;
    SocketOptions options;
    private Socket rootSocket;

    //Websockets incoming messages
    private const string WS_MESSAGE_EXPEDITION_MAP = "ExpeditionMap";
    private const string WS_MESSAGE_PLAYER_STATE = "PlayerState";
    private const string WS_MESSAGE_INIT_COMBAT = "InitCombat";
    private const string WS_MESSAGE_ENEMY_INTENTS = "EnemiesIntents";
    private const string WS_MESSAGE_PUT_DATA = "PutData";


    //Websockets outgoing messages with callback
    private const string WS_MESSAGE_GAME_SYNC = "SyncExpedition";
    
    private const string WS_MESSAGE_NODE_SELECTED = "NodeSelected";
    private const string WS_MESSAGE_CARD_PLAYED = "CardPlayed";
    private const string WS_MESSAGE_END_TURN = "EndTurn";

    private const string WS_MESSAGE_REWARD_SELECTED = "RewardSelected";
    private const string WS_MESSAGE_GET_CARD_UPGRADE_PAIR = "CardUpgradeSelected";
    private const string WS_MESSAGE_UPGRADE_CARD = "UpgradeCard";
    private const string WS_MESSAGE_CAMP_HEAL = "CampRecoverHealth";
    private const string WS_MESSAGE_MOVE_SELECTED_CARDS = "MoveCard";
    private const string WS_MESSAGE_TRINKETS_SELECTED = "TrinketsSelected";

    private const string WS_MESSAGE_USE_POTION = "UsePotion";
    private const string WS_MESSAGE_REMOVE_POTION = "RemovePotion";
    private const string WS_MESSAGE_OPEN_CHEST = "ChestOpened";
    private const string WS_MESSAGE_MERCHANT_BUY = "MerchantBuy";
    private const string WS_MESSAGE_START_ENCOUNTER_COMBAT = "CombatEncounter";
    private const string WS_MESSAGE_ENCOUNTER_SELECTION = "EncounterChoice";
    
    /*private const string WS_MESSAGE_GET_ENERGY = "GetEnergy";
    private const string WS_MESSAGE_GET_CARD_PILES = "GetCardPiles";
    private const string WS_MESSAGE_GET_PLAYER_HEALTH = "GetPlayerHealth";
    private const string WS_MESSAGE_GET_PLAYERS = "GetPlayers";
    private const string WS_MESSAGE_GET_ENEMIES = "GetEnemies";*/
    private const string WS_MESSAGE_GET_DATA = "GetData";
    private const string WS_MESSAGE_CONTINUE_EXPEDITION = "ContinueExpedition";
    private const string WS_MESSAGE_NODE_SKIP = "NodeSkipped";

    [SerializeField] private string SocketStatus = "Unknown";
    private bool doNotResuscitate = false;
    private bool SocketHealthy => manager.State == SocketManager.States.Open && rootSocket.IsOpen;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            Debug.Log($"[WebSocketManager] Socket manager Awake");
            // Turns off non-exception logging when outside of development enviroments
            HiddenConsoleManager.DisableOnBuild();

            // Connect Events
            GameManager.Instance.EVENT_EXPEDITION_SYNC.AddListener(OnRequestSync);
            GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnNodeClicked);
            GameManager.Instance.EVENT_CARD_PLAYED.AddListener(OnCardPlayed);
            GameManager.Instance.EVENT_END_TURN_CLICKED.AddListener(OnEndTurn);
            GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener(OnGenericWSDataRequest);
            GameManager.Instance.EVENT_REWARD_SELECTED.AddListener(OnRewardSelected);
            GameManager.Instance.EVENT_CONTINUE_EXPEDITION.AddListener(OnContinueExpedition);
            GameManager.Instance.EVENT_GET_UPGRADE_PAIR.AddListener(OnShowUpgradePair);
            GameManager.Instance.EVENT_USER_CONFIRMATION_UPGRADE_CARD.AddListener(OnCardUpgradeConfirmed);
            GameManager.Instance.EVENT_CAMP_HEAL.AddListener(OnCampHealSelected);
            GameManager.Instance.EVENT_CARDS_SELECTED.AddListener(OnCardsSelected);
            GameManager.Instance.EVENT_POTION_USED.AddListener(OnPotionUsed);
            GameManager.Instance.EVENT_POTION_DISCARDED.AddListener(OnPotionDiscarded);
            GameManager.Instance.EVENT_TREASURE_OPEN_CHEST.AddListener(OnTreasureOpened);
            GameManager.Instance.EVENT_MERCHANT_BUY.AddListener(OnBuyItem);
            GameManager.Instance.EVENT_ENCOUNTER_OPTION_SELECTED.AddListener(OnEncounterOptionSelected);
            GameManager.Instance.EVENT_START_COMBAT_ENCOUNTER.AddListener(OnStartCombatEncounter);
            GameManager.Instance.EVENT_SKIP_NODE.AddListener(OnSkipNode);
            GameManager.Instance.EVENT_TRINKETS_SELECTED.AddListener(OnTrinketsSelected);
        }
    }

    void Start()
    {
        if (Instance == this && (rootSocket == null || !rootSocket.IsOpen))
        {
            options = new SocketOptions();
            ConnectSocket();
        }
    }

    private void FixedUpdate()
    {
        string newSocketState = manager.State.ToString();
        if(SocketStatus != newSocketState) 
        {
            switch (manager.State) 
            {
                case SocketManager.States.Reconnecting:
                    Debug.LogWarning($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
                case SocketManager.States.Closed:
                    if (!doNotResuscitate) 
                    {
                        Debug.LogWarning($"[WebSocketManager] New Socket State: {manager.State}. Attempting Reconnect...");
                        ConnectSocket();
                        break;
                    }
                    Debug.Log($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
                default:
                    Debug.Log($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
            }
            SocketStatus = manager.State.ToString();
        }
        if (SocketHealthy && EmissionQueue.Count > 0) 
        {
            EmissionQueue.Peek().Invoke();
            EmissionQueue.Dequeue();
        } 
    }

    void OnDestroy()
    {
        doNotResuscitate = true;
        if (rootSocket != null)
        {
            Debug.Log("[WebSocket Manager] socket disconnected");
            rootSocket.Disconnect();
        }
        if (Instance == this)
        {
            Debug.Log($"[WebSocketManager] Socket manager destroyed");
        }
    }

    private void OnEnable()
    {
        Debug.Log("[WebSocketManager] Socket manager Enabled");
    }


    /// <summary>
    /// 
    /// </summary>
    void ConnectSocket()
    {
        //BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = true; 

        string token = PlayerPrefs.GetString("session_token");

        // Debug.Log("Connecting socket using token: " + token);

        SocketOptions options = new SocketOptions();
        //  options.AutoConnect = false;
        options.HTTPRequestCustomizationCallback = (manager, request) => { request.AddHeader("Authorization", token); };

        // determine the correct server the client is running on
        string hostName = Application.absoluteURL;
        string uriStr = "https://api.dev.kote.robotseamonster.com";
        //string uriStr = "https://api.alpha.knightsoftheether.com:443";

        if (hostName.IndexOf("alpha") > -1)
        {
            uriStr = "https://api.alpha.knightsoftheether.com:443";
        }

        if (hostName.IndexOf("stage") > -1)
        {
            uriStr = "https://api.stage.kote.robotseamonster.com";
        }

        if (hostName.IndexOf("dev") > -1)
        {
            uriStr = "https://api.dev.kote.robotseamonster.com";
        }


        /*string[] splitURL = hostURL.Split('.');
        string uriStr = "https://api.dev.kote.robotseamonster.com";
        if ( splitURL.Length > 1)//this will fail for localhost
        {
            switch (splitURL[1])
            {
                case "dev":
                    uriStr = "https://api.dev.kote.robotseamonster.com";
                    break;
                case "stage":
                    uriStr = "https://api.stage.kote.robotseamonster.com";
                    break;
                case "alpha":
                    uriStr = "https://api.alpha.knightsoftheether.com:443";
                    break;
                default:
                    uriStr = "https://api.stage.kote.robotseamonster.com";
                    break;
            }
        }*/

        // default to the stage server if running from the unity editor
#if UNITY_EDITOR
        uriStr = "https://api.dev.kote.robotseamonster.com";
#endif

        PlayerPrefs.SetString("ws_url", uriStr);
        Debug.Log("[WebSocket Manager] Connecting to " + uriStr);

        manager = new SocketManager(new Uri(uriStr), options);

        rootSocket = manager.Socket;
        //customNamespace = manager.GetSocket("/socket");

        rootSocket.On<Error>(SocketIOEventTypes.Error, OnError);

        rootSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);

        //customNamespace.On<string>("ExpeditionMap", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));

        rootSocket.On<string>(WS_MESSAGE_EXPEDITION_MAP, GenericParser);
        rootSocket.On<string>(WS_MESSAGE_PLAYER_STATE, GenericParser);
        rootSocket.On<string>(WS_MESSAGE_INIT_COMBAT, GenericParser);
        //rootSocket.On<string>(WS_MESSAGE_ENEMY_INTENTS, GenericParser);
        rootSocket.On<string>(WS_MESSAGE_PUT_DATA, GenericParser);


        //  manager.Open();
        Debug.Log("[WebSocketManager] Socket generated.");
    }

    #region

    private void GenericParser(string data)
    {
        SWSM_Parser.ParseJSON(data);
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Websocket Connected sucessfully!");
        
        GameManager.Instance.EVENT_WS_CONNECTED.Invoke();
    }

    void OnError(Error resp)
    {
        // Method 1: received as parameter
        Debug.Log("[WebSocket Manager] Error message: " + resp.message);

        // Method 2: access through the socket
        Debug.Log("[WebSocket Manager] Sid through socket: " + manager.Socket.Id);
    }

    private void OnRequestSync()
    {
        Emit(WS_MESSAGE_GAME_SYNC);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// 
    private void OnNodeClicked(int nodeId)
    {
        //Debug.Log("[WebSocket Manager] Sending message NodeSelected with node id " + nodeId);
        //customNamespace.Emit("NodeSelected",nodeId);

        LogEmission(WS_MESSAGE_NODE_SELECTED, nodeId);
        EmitWithResponse(OnNodeClickedAnswer, WS_MESSAGE_NODE_SELECTED, nodeId);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    private void OnNodeClickedAnswer(string data)
    {
        //NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState,WS_QUERY_TYPE.MAP_NODE_SELECTED);

        SWSM_Parser.ParseJSON(data);

        //Debug.Log("OnNodeClickedAnswer: " + nodeState);
    }


    void OnExpeditionMap(string data)
    {
#if UNITY_EDITOR
        if (GameSettings.DEBUG_MODE_ON)
        {
            data = Utils.ReadJsonFile("node_data_act1.txt");
            //data = Utils.ReadJsonFile("node_data_only_RH.txt");
            //data = Utils.ReadJsonFile("node_data_act1step2.txt");
            //data = Utils.ReadJsonFile("node_data_act_test.txt");
        }
#endif
        //Debug.Log("Data from OnExpeditionMap: " + data);
        // GameManager.Instance.EVENT_MAP_NODES_UPDATE.Invoke(data);
        SWSM_Parser.ParseJSON(data);
    }

    private void OnCardPlayed(string cardId, string id) //int enemyId)//TODO: enemyId will an array 
    {
        CardPlayedData cardData = new CardPlayedData();
        cardData.cardId = cardId;
        cardData.targetId = id;

        string data = JsonUtility.ToJson(cardData).ToString();
        //Debug.Log("[WebSocket Manager] OnCardPlayed data: " + data);

        //EmitWithResponse(OnCardPlayedAnswer, WS_MESSAGE_CARD_PLAYED, data);
        Emit(WS_MESSAGE_CARD_PLAYED, data);
    }
    private void OnCardsSelected(List<string> cardIds)
    {
        CardsSelectedList cardList = new CardsSelectedList { cardsToTake = cardIds };
        string data = JsonUtility.ToJson(cardList);
        Debug.Log("[WebSocket Manager] OnCardsSelected data: " + data);
        Emit(WS_MESSAGE_MOVE_SELECTED_CARDS, data);
    }

    private void OnTrinketsSelected(List<string> trinketIds)
    {
        string data = JsonUtility.ToJson(trinketIds);
        Debug.Log("[WebSocket Manager] OnTrinketsSelected data: " + data);
        Emit(WS_MESSAGE_TRINKETS_SELECTED, data);
    }
    
    private void OnBuyItem(string type, string id)
    {
        PurchaseData purchase = new PurchaseData() 
        {
            type = type,
            targetId = id
        };

        string data = JsonUtility.ToJson(purchase).ToString();
        //Debug.Log($"[WebSocket Manager] OnBuyItem data: {data}");

        Emit(WS_MESSAGE_MERCHANT_BUY, data);
    }


    private void OnPotionUsed(string potionId, string targetId)
    {
        PotionUsedData potionData = new PotionUsedData
        {
            potionId = potionId,
            targetId = targetId
        };
        string data = JsonUtility.ToJson(potionData);
        //Debug.Log("[WebSocket Manager] OnPotionUsed data: " + data);

        Emit(WS_MESSAGE_USE_POTION, data);
    }

    private void OnPotionDiscarded(string potionId)
    {
        //Debug.Log("[WebSocket Manager] OnDiscardPotion id: " + potionId);
        Emit(WS_MESSAGE_REMOVE_POTION, potionId);
    }

    void OnRewardSelected(string rewardId)
    {
        EmitWithResponse(WS_MESSAGE_REWARD_SELECTED, rewardId);
    }

    private void OnCampHealSelected()
    {
        EmitWithResponse(WS_MESSAGE_CAMP_HEAL);
    }

    private void OnTreasureOpened()
    {
       // Debug.Log($"[WebSocketManager] Sending message {WS_MESSAGE_OPEN_CHEST}");
        EmitWithResponse(WS_MESSAGE_OPEN_CHEST);
    }

    private void OnEncounterOptionSelected(int option)
    {
        EmitWithResponse(WS_MESSAGE_ENCOUNTER_SELECTION, option);
    }

    private void OnStartCombatEncounter()
    {
        EmitWithResponse(WS_MESSAGE_START_ENCOUNTER_COMBAT);
    }

    private void OnShowUpgradePair(string cardId)
    {
        Debug.Log($"Sending message {WS_MESSAGE_GET_CARD_UPGRADE_PAIR} with card id {cardId}");
        //customNamespace.Emit("NodeSelected",nodeId);

        EmitWithResponse(WS_MESSAGE_GET_CARD_UPGRADE_PAIR, cardId);
    }

    private void OnCardUpgradeConfirmed(string cardId)
    {
        EmitWithResponse(WS_MESSAGE_UPGRADE_CARD, cardId);
    }

    private void OnSkipNode(int nodeId)
    {
        Emit(WS_MESSAGE_NODE_SKIP, nodeId);
    }

    private void OnEndTurn()
    {
        LogEmission(WS_MESSAGE_END_TURN);
        EmitWithResponse(OnEndOfTurnAnswer, WS_MESSAGE_END_TURN);
    }

    private void OnEndOfTurnAnswer(string nodeData)
    {
        Debug.Log("on end of turn answer:" + nodeData);
        if (MessageErrorValidator.ValidateData(nodeData))
        {
            NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
            GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState, WS_QUERY_TYPE.END_OF_TURN);
        }
    }

    private void OnContinueExpedition()
    {
        EmitWithResponse(WS_MESSAGE_CONTINUE_EXPEDITION);
    }

    #endregion

    private void OnGenericWSDataRequest(WS_DATA_REQUEST_TYPES dataType)
    {
        EmitWithResponse(WS_MESSAGE_GET_DATA, dataType.ToString());
    }

    private Queue<Action> EmissionQueue = new Queue<Action>();

    private void Emit(string eventName, params object[] variables) 
    {
        if (!SocketHealthy) 
        {
            EmissionQueue.Enqueue(() => 
            {
                Emit(eventName, variables);
            });
            Debug.LogWarning($"[WebSocketManager] Socket is Unhealthy. Queuing Emission ({eventName}) for Later.");
            return;
        }
        LogEmission(eventName, variables);
        rootSocket.Emit(eventName, variables);
    }
    private void EmitWithResponse(string eventName, params object[] variables)
    {
        EmitWithResponse(GenericParser, eventName, variables);

    }

    private void EmitWithResponse(Action<string> parser, string eventName, params object[] variables)
    {
        if (!SocketHealthy)
        {
            EmissionQueue.Enqueue(() =>
            {
                EmitWithResponse(parser, eventName, variables);
            });
            Debug.LogWarning($"[WebSocketManager] Socket is Unhealthy. Queuing Emission with Response ({eventName}) for Later.");
            return;
        }
        LogEmission(eventName, variables);
        rootSocket.ExpectAcknowledgement<string>(parser).Emit(eventName, variables);

    }


#if UNITY_EDITOR
    public void ForceEmit(string eventName, params object[] variables) 
    {
        EmitWithResponse(eventName, variables);
    }
#endif

    private void LogEmission(string eventName, params object[] variables) 
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"[WebSocketManager] EMISSION >> Message: {eventName}");
        if (variables != null && variables.Length >= 1)
        {
            sb.Append($" | Action: {variables[0]}");
            for (int i = 1; i < variables.Length; i++)
            {
                sb.Append($" | Param [{i}]: {variables[i]}");
            }
        }
        Debug.Log(sb.ToString());
    }
}