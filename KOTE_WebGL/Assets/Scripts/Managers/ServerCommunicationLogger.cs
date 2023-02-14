using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommunicationLogger : SingleTon<ServerCommunicationLogger>
{
   private List<ServerCommunicationLogItem> communicationLog = new List<ServerCommunicationLogItem>();

   public void LogCommunication(string message, CommunicationDirection direction)
   {
      DateTime currentTime = DateTime.UtcNow;
      long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
      ServerCommunicationLogItem newLog = new ServerCommunicationLogItem
      {
         data = message,
         direction = direction.ToString(),
         timestamp = unixTime.ToString()
      };
      communicationLog.Add(newLog);
   }

   public List<ServerCommunicationLogItem> GetCommunicationLog()
   {
      return communicationLog;
   }
}
