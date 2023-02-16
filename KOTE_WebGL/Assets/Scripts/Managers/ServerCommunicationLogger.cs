using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ServerCommunicationLogger : SingleTon<ServerCommunicationLogger>
{
    private List<ServerCommunicationLog> communicationLog = new List<ServerCommunicationLog>();

    public void LogCommunication(string message, CommunicationDirection direction, string rawJson = "")
    {
        var newLog = BuildLog(message, direction, rawJson);
        communicationLog.Add(newLog);
    }

    private ServerCommunicationLog BuildLog(string message, CommunicationDirection direction, string rawJson = "") 
    {
        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        ServerCommunicationLog newLog = new ServerCommunicationLog
        {
            Message = message,
            Direction = direction.ToString(),
            Timestamp = unixTime.ToString()
        };
        if (!string.IsNullOrEmpty(rawJson))
        {
            newLog.Raw = JObject.Parse(rawJson);
        }
        return newLog;
    }

    public List<ServerCommunicationLog> GetCommunicationLog()
    {
        return communicationLog;
    }
   
    [Serializable]
    public class ServerCommunicationLog
    {
        [JsonProperty("timestamp")]
        public string Timestamp;
        [JsonProperty("direction")]
        public string Direction;
        [JsonProperty("message")]
        public string Message;
        [JsonProperty("raw")]
        public JObject Raw = new JObject();
    }
}
