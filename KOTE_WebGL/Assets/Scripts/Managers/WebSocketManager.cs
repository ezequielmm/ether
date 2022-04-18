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

    // Start is called before the first frame update
    void Start()
    {
        //events
        GameManager.Instance.EVENT_MAP_NODE_SELECTED.AddListener(OnNodeClicked);

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
        options.HTTPRequestCustomizationCallback = (manager, request) =>
        {
            request.AddHeader("Authorization",token);
        };

        //options.AdditionalQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string>();
        //options.AdditionalQueryParams.Add("transports", "['websocket']");


        //string uriStr = "https://45.33.0.125:8443";
        //string uriStr = "https://delcasda.com:8443";
        //string uriStr = "https://delcasda.com:8888";

         string uriStr = "https://api.game.kote.robotseamonster.com:443";
        /*
 #if UNITY_EDITOR

         uriStr = "wss://api.game.kote.robotseamonster.com:7777";
 #endif*/
        manager = new SocketManager(new Uri(uriStr), options);

        rootSocket = manager.Socket;
        //customNamespace = manager.GetSocket("/socket");

        rootSocket.On<Error>(SocketIOEventTypes.Error, OnError);

        rootSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);

        //customNamespace.On<string>("ExpeditionMap", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));
        rootSocket.On<string>("ExpeditionMap", OnExpeditionMap);
        rootSocket.On<string>("PlayerState", OnPlayerState);
      
        //  manager.Open();
  
    }

    private void OnHello(string obj)
    {
        Debug.Log(obj);
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Websocket Connected sucessfully!");

        // Method 1: received as parameter
        //Debug.Log("Sid through parameter: " + resp.sid);
        //Debug.Log("manager.Handshake.Sid: " + manager.Handshake.Sid);

        // Method 2: access through the socket
        //Debug.Log("Sid through socket: " + manager.Socket.Id);
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

        rootSocket.ExpectAcknowledgement<string>(OnNodeClickedAnswer).Emit("NodeSelected", nodeId);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="test"></param>

    private void OnNodeClickedAnswer(string nodeData)
    {
        Debug.Log("Test answer " + nodeData);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.Invoke(nodeData);
    }


    void OnExpeditionMap(string data)
    {
       
        Debug.Log("Data from OnExpeditionMap: " + data);
        GameManager.Instance.EVENT_MAP_NODES_UPDATE.Invoke(data);
    }

    void OnPlayerState(string data)
    {
        Debug.Log("Data from OnPlayerState: " + data);
    }



}
