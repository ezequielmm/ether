using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommunicationLogger : SingleTon<ServerCommunicationLogger>
{
   private List<ServerCommunicationLog> communicationLog = new List<ServerCommunicationLog>();

   public void LogCommunication(string message, CommunicationDirection direction)
   {
      DateTime currentTime = DateTime.UtcNow;
      long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
      ServerCommunicationLog newLog = new ServerCommunicationLog
      {
         data = message,
         direction = direction.ToString(),
         timestamp = unixTime.ToString()
      };
      communicationLog.Add(newLog);
   }

   public List<ServerCommunicationLog> GetCommunicationLog()
   {
      return communicationLog;
   }
   
   [Serializable]
   public class ServerCommunicationLog
   {
      public string timestamp;
      public string direction;
      public string data;
   }
}
