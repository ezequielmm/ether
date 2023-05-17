using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestingSocket : MonoBehaviour
    {
        public string Token;
        
        private void Awake()
        {
            var url = "http://localhost:3000";
            
            // Configurar el SocketManager
            SocketOptions options = new SocketOptions();
            //options.AutoConnect = false;

            var socketManager = new SocketManager(new System.Uri(url), options);
            options.HTTPRequestCustomizationCallback = (manager, request) =>
            {
                request.AddHeader("Authorization", Token);
            };

            // Obtener el socket principal
            var socket = socketManager.Socket;

            // Suscribirse a eventos de conexión y desconexión
            socket.On<Error>(SocketIOEventTypes.Error, (err) =>
            {
                Debug.Log($"Error: {err}"); 
            });
            socket.On<ConnectResponse>(SocketIOEventTypes.Connect, (conn) =>
            {
                Debug.Log($"Connected! {conn}");
            });
            // ... suscribirse a otros eventos aquí

            // Conectar al servidor Socket.IO
            socketManager.Open();
        }
    }
}