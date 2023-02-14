using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommunicationLogger : SingleTon<ServerCommunicationLogger>
{
   private List<LogItem> communicationLog = new List<LogItem>();

   public void LogCommunication(string message, CommunicationDirection direction)
   {
      DateTime currentTime = DateTime.UtcNow;
      long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
      LogItem newLog = new LogItem
      {
         data = message,
         direction = direction.ToString(),
         timestamp = unixTime.ToString()
      };
      communicationLog.Add(newLog);
   }

   public List<LogItem> GetCommunicationLog()
   {
      return communicationLog;
   }
}
