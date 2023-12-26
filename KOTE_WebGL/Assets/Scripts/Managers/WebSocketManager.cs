using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class WebSocketManager : SingleTon<WebSocketManager>
{

    public static string ClientId;
    SocketManager manager;
    SocketOptions options;
    private Socket rootSocket;

    //Websockets incoming messages
    private const string WS_MESSAGE_EXPEDITION_MAP = "ExpeditionMap";
    private const string WS_MESSAGE_PLAYER_STATE = "PlayerState";
    private const string WS_MESSAGE_INIT_COMBAT = "InitCombat";
    private const string WS_MESSAGE_ENEMY_INTENTS = "EnemiesIntents";
    private const string WS_MESSAGE_PUT_DATA = "PutData";
    private const string WS_MESSAGE_REWARD_LIST = "RewardList";


    [SerializeField] private string SocketStatus = "Unknown";
    private bool doNotResuscitate = true;
    public bool SocketOpened { get; private set; } = false;

    public bool IsSocketHealthy
    {
        get
        {
            if (manager != null && rootSocket != null)
            {
                return manager.State == SocketManager.States.Open && rootSocket != null && rootSocket.IsOpen &&
                       Time.time - socketOpenTimeGameSeconds > 1;
            }

            return false;
        }
    }

    private float socketOpenTimeGameSeconds = -1;
    private float socketDeathTimeGameSeconds = -1;

    private Queue<Action> EmissionQueue = new Queue<Action>();
    private List<string> rewardCached = new();
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            Debug.Log($"[WebSocketManager] Socket manager Awake");

            // Connect Events
            GameManager.Instance.EVENT_EXPEDITION_SYNC.AddListener(OnRequestSync);
            //GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnNodeClicked);
            GameManager.Instance.OnNodeTransitionEnd.AddListener(OnNodeClicked); // TODO: A transition now controls the flow of the map
            GameManager.Instance.EVENT_CARD_PLAYED.AddListener(OnCardPlayed);
            GameManager.Instance.EVENT_END_TURN_CLICKED.AddListener(OnEndTurn);
            GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener(OnGenericWSDataRequest);
            GameManager.Instance.EVENT_REWARD_SELECTED.AddListener(OnRewardSelected);
            GameManager.Instance.EVENT_CONTINUE_EXPEDITION.AddListener(OnContinueExpedition);
            GameManager.Instance.EVENT_CAMP_HEAL.AddListener(OnCampHealSelected);
            GameManager.Instance.EVENT_POTION_USED.AddListener(OnPotionUsed);
            GameManager.Instance.EVENT_POTION_DISCARDED.AddListener(OnPotionDiscarded);
            GameManager.Instance.EVENT_TREASURE_OPEN_CHEST.AddListener(OnTreasureOpened);
            GameManager.Instance.EVENT_MERCHANT_BUY.AddListener(OnBuyItem);
            GameManager.Instance.EVENT_START_COMBAT_ENCOUNTER.AddListener(OnStartCombatEncounter);
            GameManager.Instance.EVENT_SKIP_NODE.AddListener(OnSkipNode);
            GameManager.Instance.EVENT_TRINKETS_SELECTED.AddListener(OnTrinketsSelected);

            GameManager.Instance.EVENT_SCENE_LOADED.AddListener(OnSceneChange);
            
            GameManager.instance.EVENT_POPULATE_REWARDS_PANEL.AddListener(ClearRewardsCache);
        }
    }

    private void ClearRewardsCache(SWSM_RewardsData data)
    {
        Debug.Log($"Rewards panel populated, clearing cache");
        var rewardItemDatas = data.data.data.rewards.Select(e => e.id);
        rewardCached.RemoveAll(e => !rewardItemDatas.Contains(e));
    }

    void Start()
    {
        if (Instance == this && (rootSocket == null || !rootSocket.IsOpen))
        {
            options = new SocketOptions();
            ConnectSocket();
        }
    }

    private void OnSceneChange(inGameScenes scene)
    {
        if (scene == inGameScenes.MainMenu)
        {
            doNotResuscitate = true;
            this.DestroyInstance();
        }
    }

    private void FixedUpdate()
    {
        UpdateSocketHealthStatus();
        CheckTimedoutSocket();
        ManageQueuedMessages();
    }

    protected override void OnDestroy()
    {
        doNotResuscitate = true;
        if (rootSocket != null)
        {
            Debug.Log("[WebSocket Manager] socket disconnected");
            rootSocket.Disconnect();
        }

        if (instance == this)
        {
            Debug.Log($"[WebSocketManager] Socket manager destroyed");
        }
        base.OnDestroy();
    }

    private void OnEnable()
    {
        Debug.Log("[WebSocketManager] Socket manager Enabled");
    }

    private void UpdateSocketHealthStatus()
    {
        string newSocketState = manager.State.ToString();
        if (SocketStatus != newSocketState)
        {
            // Logs
            switch (manager.State)
            {
                case SocketManager.States.Closed:
                    Debug.LogError($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
                case SocketManager.States.Reconnecting:
                    Debug.LogError($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
                default:
                    Debug.Log($"[WebSocketManager] New Socket State: {manager.State}.");
                    break;
            }

            // Actions
            switch (manager.State)
            {
                case SocketManager.States.Reconnecting:
                    socketDeathTimeGameSeconds = Time.time;
                    break;
                case SocketManager.States.Closed:
                    if (!doNotResuscitate)
                    {
                        socketDeathTimeGameSeconds = Time.time;
                        Debug.Log($"[WebSocketManager] Trying to fix socket.");
                        ConnectSocket();
                    }
                    break;
                case SocketManager.States.Open:
                    SocketOpened = true;
                    socketOpenTimeGameSeconds = Time.time;
                    socketDeathTimeGameSeconds = -1;
                    doNotResuscitate = false;
                    break;
            }

            // Update Unity UI
            SocketStatus = manager.State.ToString();
        }
    }

    private void CheckTimedoutSocket()
    {
        if (!IsSocketHealthy && socketDeathTimeGameSeconds != -1 &&
            Time.time - socketDeathTimeGameSeconds > GameSettings.MAX_TIMEOUT_SECONDS)
        {
            // After some seconds of closed connection, return to main menu.
            Debug.LogError(
                $"[WebSocketManager] Disconnected for {Mathf.Round(Time.time - socketDeathTimeGameSeconds)} seconds Connection could not be salvaged.");
            Destroy(gameObject);
        }
    }

    private void ManageQueuedMessages()
    {
        if (IsSocketHealthy && EmissionQueue.Count > 0 && Time.time - socketOpenTimeGameSeconds > 1)
        {
            EmissionQueue.Peek().Invoke();
            EmissionQueue.Dequeue();
        }
    }


    void ConnectSocket()
    {
        //BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = true; 

        string token = AuthenticationManager.Instance.GetSessionToken();

        // Debug.Log("Connecting socket using token: " + token);

        SocketOptions options = new SocketOptions();
        //  options.AutoConnect = false;
        options.HTTPRequestCustomizationCallback = (manager, request) =>
        {
            request.AddHeader("Authorization", token);
            request.AddHeader("UserAddress", AuthenticationManager.LoginData.Wallet);
           // request.AddHeader("useraddress", AuthenticationManager.LoginData.Wallet);

        };

        string uriStr = ClientEnvironmentManager.Instance.WebSocketURL;

        PlayerPrefs.SetString("ws_url", uriStr);
        Debug.Log("[WebSocket Manager] Connecting to " + uriStr);

        manager = new SocketManager(new Uri(uriStr), options);
       
        Debug.Log($"Socket connected: ID: {manager?.Socket?.Id} Handshakes SID: {manager?.Handshake?.Sid} ");


        rootSocket = manager.Socket;
        //customNamespace = manager.GetSocket("/socket");

        rootSocket.On<Error>(SocketIOEventTypes.Error, OnError);

        rootSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);

        //customNamespace.On<string>("ExpeditionMap", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));

        rootSocket.On<string>(WS_MESSAGE_EXPEDITION_MAP, GenericParserWithoutCallback);
        rootSocket.On<string>(WS_MESSAGE_PLAYER_STATE, GenericParserWithoutCallback);
        rootSocket.On<string>(WS_MESSAGE_INIT_COMBAT, GenericParserWithoutCallback);
        //rootSocket.On<string>(WS_MESSAGE_ENEMY_INTENTS, GenericParserWithoutCallback);
        rootSocket.On<string>(WS_MESSAGE_PUT_DATA, GenericParserWithoutCallback);
        rootSocket.On<string>(WS_MESSAGE_REWARD_LIST, GenericParserWithoutCallback);


        //  manager.Open();
        Debug.Log("[WebSocketManager] Socket generated.");
    }

    #region

    private void GenericParser(string data)
    {
        try
        {
            WebSocketParser.ParseJSON(data);
        } 
        catch(Exception e) 
        {
            Debug.LogException(e);
        }
    }

    private void GenericParserWithoutCallback(string data)
    {
        LogIncoming(data);
        GenericParser(data);
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Websocket Connected sucessfully! SID " + resp.sid);
        ClientId = resp.sid;
        GameManager.Instance.EVENT_WS_CONNECTED.Invoke();
    }

    void OnError(Error resp)
    {
        // Method 1: received as parameter
        Debug.LogError("[WebSocket Manager] Error message: " + resp.message);

        // Method 2: access through the socket
        Debug.LogError("[WebSocket Manager] Sid through socket: " + manager.Socket.Id);
        Debug.LogError(resp);
    }

    private void OnRequestSync()
    {
        Emit(SocketEvent.GameSync);
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

        EmitWithResponse(OnNodeClickedAnswer, SocketEvent.NodeSelected, nodeId);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>
    private void OnNodeClickedAnswer(string data)
    {
        //NodeStateData nodeState = JsonConvert.DeserializeObject<NodeStateData>(nodeData);
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState,WS_QUERY_TYPE.MAP_NODE_SELECTED);

        GenericParser(data);

        //Debug.Log("OnNodeClickedAnswer: " + nodeState);
    }


    void OnExpeditionMap(string data)
    {
#if UNITY_EDITOR
        if (GameSettings.DEBUG_MODE_ON)
        {
#pragma warning disable CS0162 // Unreachable code detected
            data = Utils.ReadJsonFile("node_data_act1.txt");
#pragma warning restore CS0162 // Unreachable code detected
                              //data = Utils.ReadJsonFile("node_data_only_RH.txt");
                              //data = Utils.ReadJsonFile("node_data_act1step2.txt");
                              //data = Utils.ReadJsonFile("node_data_act_test.txt");
        }
#endif
        //Debug.Log("Data from OnExpeditionMap: " + data);
        // GameManager.Instance.EVENT_MAP_NODES_UPDATE.Invoke(data);
        GenericParser(data);
    }

    private void OnCardPlayed(string cardId, string id) //int enemyId)//TODO: enemyId will an array 
    {
        CardPlayedData cardData = new CardPlayedData();
        cardData.cardId = cardId;
        cardData.targetId = id;

        string data = JsonConvert.SerializeObject(cardData).ToString();
        //Debug.Log("[WebSocket Manager] OnCardPlayed data: " + data);

        //EmitWithResponse(OnCardPlayedAnswer, WS_MESSAGE_CARD_PLAYED, data);
        Emit(SocketEvent.CardPlayed, data);
    }

    private void OnTrinketsSelected(List<string> trinketIds)
    {
        string data = JsonConvert.SerializeObject(trinketIds);
        Debug.Log("[WebSocket Manager] OnTrinketsSelected data: " + data);
        Emit(SocketEvent.TrinketsSelected, data);
    }

    private void OnBuyItem(string type, string id)
    {
        PurchaseData purchase = new PurchaseData()
        {
            type = type,
            targetId = id
        };

        string data = JsonConvert.SerializeObject(purchase).ToString();
        //Debug.Log($"[WebSocket Manager] OnBuyItem data: {data}");

        Emit(SocketEvent.MerchantBuy, data);
    }


    private void OnPotionUsed(string potionId, string targetId)
    {
        PotionUsedData potionData = new PotionUsedData
        {
            potionId = potionId,
            targetId = targetId
        };
        string data = JsonConvert.SerializeObject(potionData);
        //Debug.Log("[WebSocket Manager] OnPotionUsed data: " + data);

        Emit(SocketEvent.UsePotion, data);
    }

    private void OnPotionDiscarded(string potionId)
    {
        //Debug.Log("[WebSocket Manager] OnDiscardPotion id: " + potionId);
        Emit(SocketEvent.RemovePotion, potionId);
    }

    void OnRewardSelected(string rewardId)
    {
        Debug.Log($"rewardID: {rewardId}");
        if (rewardCached.Contains(rewardId))
        {
            Debug.LogError($"Already queueing reward {rewardId}!");
            return;
        }
        rewardCached.Add(rewardId);
        Emit(SocketEvent.RewardSelected, rewardId);
    }

    private void OnCampHealSelected()
    {
        EmitWithResponse(SocketEvent.CampHeal);
    }

    private void OnTreasureOpened()
    {
        // Debug.Log($"[WebSocketManager] Sending message {WS_MESSAGE_OPEN_CHEST}");
        EmitWithResponse(SocketEvent.OpenChest);
    }

    private void OnStartCombatEncounter()
    {
        EmitWithResponse(SocketEvent.StartCombat);
    }

    private void OnSkipNode(int nodeId)
    {
        Emit(SocketEvent.NodeSkip, nodeId);
    }

    private void OnEndTurn()
    {
        Debug.Log($"[CombatQueue] Sending message {SocketEvent.EndTurn} [OnEndTurn]");
        EmitWithResponse(OnEndOfTurnAnswer, SocketEvent.EndTurn);
    }

    private void OnEndOfTurnAnswer(string nodeData)
    {
        Debug.Log("on end of turn answer:" + nodeData);
        if (MessageErrorValidator.ValidateData(nodeData))
        {
            NodeStateData nodeState = JsonConvert.DeserializeObject<NodeStateData>(nodeData);
            GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState, WS_QUERY_TYPE.END_OF_TURN);
        }
    }

    private void OnContinueExpedition()
    {
        EmitWithResponse(SocketEvent.ContinueExpedition);
    }

    #endregion

    private void OnGenericWSDataRequest(WS_DATA_REQUEST_TYPES dataType)
    {
        EmitWithResponse(SocketEvent.GetData, dataType.ToString());
    }

    private void ResolvePromise(string json, UniPromise<string> promise)
    {
        promise.FulfillRequest(json);
    }

    private UniPromise<string> CreatePromise()
    {
        UniPromise<string> promise = new UniPromise<string>();
        return promise;
    }

    public UniTask SendData(string eventName, object message) 
    {
        string rawJson = JsonConvert.SerializeObject(message);
        Emit(eventName, rawJson);
        return UniTask.CompletedTask;
    }

    private void Emit(string eventName, params object[] variables)
    {
        if (!IsSocketHealthy)
        {
            EmissionQueue.Enqueue(() => { Emit(eventName, variables); });
            Debug.LogWarning($"[WebSocketManager] Socket is Unhealthy. Queuing Emission ({eventName}) for Later.");
            return;
        }
        // stop logging
        // LogEmission(eventName, variables);

        if (variables.Length > 0)
        {
            var rewardID = (string) variables[0];
            Debug.Log($"EMIT: {rewardID}");
        }
        
        rootSocket.Emit(eventName, variables);
    }

    private void EmitWithResponse(string eventName, params object[] variables)
    {
        EmitWithResponse(GenericParser, eventName, variables);
    }

    public async UniTask<string> EmitAwaitResponse(string eventName, params object[] variables)
    {

        UniPromise<string> promise = CreatePromise();
        if (IsSocketHealthy)
        {
            EmitPromise(promise, eventName, variables);
        }
        else
        {
            EmissionQueue.Enqueue(() => { EmitPromise(promise, eventName, variables); });
            Debug.LogWarning(
                $"[WebSocketManager] Socket is Unhealthy. Queuing Emission with Response ({eventName}) for Later.");
        }

        await promise.WaitForFufillment();
        Debug.Log($"[WebSocketManager] RESPONSE [{promise.Id.ToString().Substring(0,4)}] <<< {promise.Data}");
      //  ServerCommunicationLogger.Instance.LogCommunication($"[WebSocketManager] RESPONSE [{promise.Id.ToString().Substring(0, 4)}] <<<",
    //        CommunicationDirection.Incoming, promise.Data);
        return promise.Data;
    }

    private void EmitPromise(UniPromise<string> promise, string eventName, params object[] variables)
    {
        // stop log emit promise
       // LogEmissionExpectingResponse(promise.Id, eventName, variables);
        rootSocket.ExpectAcknowledgement<string>((json) => { ResolvePromise(json, promise); })
            .Emit(eventName, variables);
    }

    private void EmitWithResponse(Action<string> parser, string eventName, params object[] variables)
    {
        if (!IsSocketHealthy)
        {
            EmissionQueue.Enqueue(() => { EmitWithResponse(parser, eventName, variables); });
            Debug.LogWarning(
                $"[WebSocketManager] Socket is Unhealthy. Queuing Emission with Response ({eventName}) for Later.");
            return;
        }
        Guid requestId = Guid.NewGuid();
       // stop logging
       // LogEmissionExpectingResponse(requestId, eventName, variables);
        rootSocket.ExpectAcknowledgement<string>((json) => 
        {
            LogResponse(requestId, json);
            parser(json);
        }).Emit(eventName, variables);
    }


#if UNITY_EDITOR
    public void ForceEmit(string eventName, params object[] variables)
    {
        EmitWithResponse(eventName, variables);
    }
#endif

    private void LogResponse(Guid requestId, string rawJson) 
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
       // Debug.Log($"[WebSocketManager] RESPONSE [{requestIdShortened}] <<< {rawJson}");
        //LogHelper.SendIncomingCommunicationLogs($"[WebSocketManager] RESPONSE [{requestIdShortened}] <<<", rawJson);
    }

    private void LogIncoming(string rawJson)
    {
        //Debug.Log($"[WebSocketManager] INCOMING <<< {rawJson}");
       // LogHelper.SendIncomingCommunicationLogs($"[WebSocketManager] INCOMING <<<", rawJson);
    }

    private void LogEmission(string eventName, params object[] variables)
    {
        string variableString = LogHelper.VariablesToHumanReadable(eventName, variables);
        string jsonString = LogHelper.VariablesToJson(eventName, variables);
       // Debug.Log($"[WebSocketManager] EMISSION >>> {jsonString}");
      //  LogHelper.SendOutgoingCommunicationLogs($"[WebSocketManager] EMISSION >>> {variableString}", jsonString);
    }

    private void LogEmissionExpectingResponse(Guid requestId, string eventName, params object[] variables)
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
        string variableString = LogHelper.VariablesToHumanReadable(eventName, variables);
        string jsonString = LogHelper.VariablesToJson(eventName, variables);
       // Debug.Log($"[WebSocketManager] EMISSION [{requestIdShortened}] >>> {jsonString}");
       // LogHelper.SendOutgoingCommunicationLogs($"[WebSocketManager] EMISSION [{requestIdShortened}] >>> {variableString}", jsonString);
    }
}

public static class SocketEvent
{
    public static readonly string GameSync = "SyncExpedition";

    public static readonly string NodeSelected = "NodeSelected";
    public static readonly string CardPlayed = "CardPlayed";
    public static readonly string EndTurn = "EndTurn";

    public static readonly string RewardSelected = "RewardSelected";
    public static readonly string GetCardUpgradePair = "CardUpgradeSelected";
    public static readonly string UpgradeCard = "UpgradeCard";
    public static readonly string CampHeal = "CampRecoverHealth";
    public static readonly string MoveSelectedCard = "MoveCard";
    public static readonly string TrinketsSelected = "TrinketsSelected";

    public static readonly string UsePotion = "UsePotion";
    public static readonly string RemovePotion = "RemovePotion";
    public static readonly string OpenChest = "ChestOpened";
    public static readonly string MerchantBuy = "MerchantBuy";
    public static readonly string StartCombat = "CombatEncounter";
    public static readonly string EncounterSelection = "EncounterChoice";

    /*public static readonly string GetEnergy = "GetEnergy";
    public static readonly string GetCardPiles = "GetCardPiles";
    public static readonly string GetPlayerHealth = "GetPlayerHealth";
    public static readonly string GetPlayers = "GetPlayers";
    public static readonly string GetEnemies = "GetEnemies";*/
    public static readonly string GetData = "GetData";
    public static readonly string ContinueExpedition = "ContinueExpedition";
    public static readonly string NodeSkip = "NodeSkipped";
}
