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
      //  ConnectSocket(); //Disabled connection until actual implementation
      
    }

    // Update is called once per frame
    void ConnectSocket()
    {
        SocketOptions options = new SocketOptions();
      //  options.AutoConnect = false;
        options.HTTPRequestCustomizationCallback = (manager, request) =>
        {
            request.AddHeader("Authorization", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIxIiwianRpIjoiOGVlN2VhY2Y4MjgzOWUxYjE4ZmQ3MjMzYzYzMWYzYzlmZDdlN2IwMWE0ZDY1ZTZhMWRmMWJiZTUxZmE2OWU5N2Q1N2YyN2IxOGVkZmQ1MDQiLCJpYXQiOjE2NDU3MjE5MDYuNzUzOTM3LCJuYmYiOjE2NDU3MjE5MDYuNzUzOTQxLCJleHAiOjE2NzcyNTc5MDYuNzQ4OTA1LCJzdWIiOiIyIiwic2NvcGVzIjpbXX0.3LC_36H2UAsJbBvUIZ_NHZibKuKz0AIhVgC1a0oTL8ZEYT5HU1foWeYyI7-pexJiPlMKfvI1K2h7BebjNqP5LP0bT7PVGxi-X3ZmzGiKj1mXCzOouI_puRZXAm9POJqQ3FpN8zgOvLMFbxu1FIg4-bd4wPk3Y8vp5YFawdETJRduvDBCOEEuYVHFCq97zqS6pZ4sqC0eyRUYv_p6zCypI5ZHzIWRMhlS1lHmEwV6n9yo_4ivd5PwpezXvchX3nYLN4Lz5uIEFNdRBfuwqWqaolI0HNoYtsaeo4zdMwG4OcaH4dNvTUWmnCw835ixQMm59HNz8VnaWDtniz73EVRoZtUrbLVxjQ1jH3CdWzYVfy6G1TjVGzW3t5Ggq_L7MNyI9occ_l98EifphAgn3O_1ETKK1QBRep_H6cx1-1WNTYrmvSLjqPf9t5p1ZsnhAbNNHhU4hZapM8cn16CERsViiSBeq4N8WbAq_ZKOoPX7JMU5VbDC_5XVs1MckkXBDOEbHEohE2Il9my003-XCm6EwWHcA6fD5-p0UmqRy_qb6MpbNpJmrdQOZoBfq_Plko9O0lybedn_JBw6qUi0kPxP1mzWjkFS_DfecTlKYQr55g43Hnm_Ohs-2_Rv60zjwO3AMVBhLipq7HlnYqLIQiLuuqDAhI4ZFQIiDvfREcZnuZE");
        };

        manager = new SocketManager(new Uri("http://api.game.kote.robotseamonster.com:7777"), options);
             
        

        var root = manager.Socket;
        var customNamespace = manager.GetSocket("/socket");

        root.On<Error>(SocketIOEventTypes.Error, OnError);

        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
       
        customNamespace.On<string>("ReceiveExpeditionStatus", (arg1) => Debug.Log("Data from ReceiveExpeditionStatus:" + arg1));


        //  manager.Open();

  
    }

    void OnConnected(ConnectResponse resp)
    {
       
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



}
