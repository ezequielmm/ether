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
    // Start is called before the first frame update
    void Start()
    {
        options = new SocketOptions();
        ConnectSocket(); //Disabled connection until actual implementation
      
    }

    // Update is called once per frame
    void ConnectSocket()
    {       
        string token = PlayerPrefs.GetString("session_token");

        Debug.Log("Connecting socket using token: " + token);

        SocketOptions options = new SocketOptions();
      //  options.AutoConnect = false;
        options.HTTPRequestCustomizationCallback = (manager, request) =>
        {
            request.AddHeader("Authorization",token);
        };

        manager = new SocketManager(new Uri("http://api.game.kote.robotseamonster.com:7777"), options);             
        

        var root = manager.Socket;
        var customNamespace = manager.GetSocket("/socket");

        root.On<Error>(SocketIOEventTypes.Error, OnError);

        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
       
        //customNamespace.On<string>("ExpeditionMap", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));
        customNamespace.On<string>("ExpeditionMap", OnExpeditionMap);
        root.On<string>("ExpeditionMap", OnExpeditionMapRoot);


        //  manager.Open();

  
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Websocket Connected sucessfully!");

        // Method 1: received as parameter
        Debug.Log("Sid through parameter: " + resp.sid);
        Debug.Log("manager.Handshake.Sid: " + manager.Handshake.Sid);

        // Method 2: access through the socket
        Debug.Log("Sid through socket: " + manager.Socket.Id);
    }

    void OnError(Error resp)
    {
        // Method 1: received as parameter
        Debug.Log("Error message: " + resp.message);

        // Method 2: access through the socket
        Debug.Log("Sid through socket: " + manager.Socket.Id);
    }

    void OnExpeditionMap(string data)
    {
        Debug.Log("Data from OnExpeditionMap: " + data);
    }

    void OnExpeditionMapRoot(string data)
    {
        Debug.Log("Data from OnExpeditionMap: " + data);
    }



}
