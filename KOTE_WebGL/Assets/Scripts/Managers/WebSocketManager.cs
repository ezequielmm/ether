using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;

public class WebSocketManager : MonoBehaviour
{
    SocketManager manager;
    SocketOptions options;
    private Socket rootSocket;

    //Websockets incoming messages
    private const string WS_MESSAGE_EXPEDITION_MAP = "ExpeditionMap";
    private const string WS_MESSAGE_PLAYER_STATE = "PlayerState";
    private const string WS_MESSAGE_INIT_COMBAT = "InitCombat";   

    //Websockets outgoing messages with callback
    private const string WS_MESSAGE_NODE_SELECTED = "NodeSelected";
    private const string WS_MESSAGE_CARD_PLAYED = "CardPlayed";
    private const string WS_MESSAGE_END_TURN = "EndTurn";
    private const string WS_MESSAGE_GET_ENERGY = "GetEnergy";

    // Start is called before the first frame update
    void Start()
    {
        //events
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnNodeClicked);
        GameManager.Instance.EVENT_CARD_PLAYED.AddListener(OnCardPlayed);
        GameManager.Instance.EVENT_END_TURN_CLICKED.AddListener(OnEndTurn);

        GameManager.Instance.EVENT_GET_ENERGY.AddListener(OnEnergyRequest);

        options = new SocketOptions();
        ConnectSocket(); //Disabled connection until actual implementation
    }


    /// <summary>
    /// 
    /// </summary>
    void ConnectSocket()
    {
        //BestHTTP.HTTPManager.UseAlternateSSLDefaultValue = true; 

        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("Connecting socket using token: " + token);

        SocketOptions options = new SocketOptions();
        //  options.AutoConnect = false;
        options.HTTPRequestCustomizationCallback = (manager, request) => { request.AddHeader("Authorization", token); };

        // determine the correct server the client is running on
        string hostURL = Application.absoluteURL;
        string[] splitURL = hostURL.Split('.');
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
                default:
                    uriStr = "https://api.stage.kote.robotseamonster.com";
                    break;
            }
        }
        // default to the stage server if running from the unity editor
#if UNITY_EDITOR
        uriStr = "https://api.dev.kote.robotseamonster.com";
#endif

        Debug.Log("Connecting to "+uriStr);

        manager = new SocketManager(new Uri(uriStr), options);

        rootSocket = manager.Socket;
        //customNamespace = manager.GetSocket("/socket");

        rootSocket.On<Error>(SocketIOEventTypes.Error, OnError);

        rootSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);

        //customNamespace.On<string>("ExpeditionMap", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));
        rootSocket.On<string>(WS_MESSAGE_EXPEDITION_MAP, GenericParser);
        rootSocket.On<string>(WS_MESSAGE_PLAYER_STATE, GenericParser);
        rootSocket.On<string>(WS_MESSAGE_INIT_COMBAT, GenericParser);
      
        //  manager.Open();
    }

    private void GenericParser(string data)
    {
        SWSM_Parser.ParseJSON(data);
    }

    private void OnHello(string obj)
    {
        Debug.Log(obj);
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Websocket Connected sucessfully!");
    }

    void OnError(Error resp)
    {
        // Method 1: received as parameter
        Debug.Log("Error message: " + resp.message);

        // Method 2: access through the socket
        Debug.Log("Sid through socket: " + manager.Socket.Id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// 
    private void OnNodeClicked(int nodeId)
    {
        Debug.Log("Sending message NodeSelected with node id " + nodeId);
        //customNamespace.Emit("NodeSelected",nodeId);

        rootSocket.ExpectAcknowledgement<string>(OnNodeClickedAnswer).Emit(WS_MESSAGE_NODE_SELECTED, nodeId);
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
        Debug.Log("Data from OnExpeditionMap: " + data);
        // GameManager.Instance.EVENT_MAP_NODES_UPDATE.Invoke(data);
        SWSM_Parser.ParseJSON(data);

      
    }

    void OnPlayerState(string data)
    {
        PlayerStateData playerState = JsonUtility.FromJson<PlayerStateData>(data); //TODO: move this to websocker manager
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(playerState);
        Debug.Log("Data from OnPlayerState: " + playerState);
    }

    private void OnCardPlayed(string cardId)
    {
        CardPlayedData obj = new CardPlayedData();
        //  obj.card_id = "87d501f6-0583-484c-bf1d-d09d822c68fa";
        obj.card_id = cardId;

        string data = JsonUtility.ToJson(obj).ToString();
        Debug.Log("sending WS playedcard test=" + data);

        rootSocket.ExpectAcknowledgement<string>(OnCardPlayedAnswer).Emit(WS_MESSAGE_CARD_PLAYED, data);
    }

    private void OnCardPlayedAnswer(string nodeData)
    {
        Debug.Log("on card played answer:" + nodeData);
        SWSM_Parser.ParseJSON(nodeData);
        if (MessageErrorValidator.ValidateData(nodeData))
        {
            NodeStateData nodeState = JsonUtility.FromJson<NodeStateData>(nodeData);
            GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeState, WS_QUERY_TYPE.CARD_PLAYED);
        }
    }

    //END of Turn

    private void OnEndTurn()
    {
        rootSocket.ExpectAcknowledgement<string>(OnEndOfTurnAnswer).Emit(WS_MESSAGE_END_TURN);
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

    private void OnEnergyRequest()
    {
        rootSocket.ExpectAcknowledgement<int[]>(OnEnergyRequestRespond).Emit(WS_MESSAGE_GET_ENERGY);
    }

    private void OnEnergyRequestRespond(int[] data)
    {
        Debug.Log("[OnEnergyRequest]"+data[0]+","+data[1]);
        GameManager.Instance.EVENT_UPDATE_ENERGY.Invoke(data[0],data[1]);
    }
}