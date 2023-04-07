using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LogHelper
{
    public static readonly int LengthOfIdToLog = 4;
    public static void SendOutgoingCommunicationLogs(string stringLog, string rawJson)
    {
        ServerCommunicationLogger.Instance.LogCommunication(stringLog, CommunicationDirection.Outgoing, rawJson);
    }

    public static void SendIncomingCommunicationLogs(string stringLog, string rawJson)
    {
        ServerCommunicationLogger.Instance.LogCommunication(stringLog, CommunicationDirection.Incoming, rawJson);
    }

    private class OutgoingMessage
    {
        public object eventName;
        public object[] variables;
    }

    public static string VariablesToHumanReadable(string eventName, params object[] variables)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Event: {eventName}");
        if (variables != null && variables.Length >= 1)
        {
            for (int i = 0; i < variables.Length; i++)
            {
                sb.Append($" | Param [{i}]: {variables[i]}");
            }
        }
        return sb.ToString();
    }

    public static string VariablesToJson(string eventName, params object[] variables)
    {
        OutgoingMessage temporaryContainer = new OutgoingMessage();

        temporaryContainer.eventName = eventName;
        temporaryContainer.variables = variables;
        try
        {
            return JsonConvert.SerializeObject(temporaryContainer);
        }
        catch (System.Exception e) 
        {
            Debug.LogWarning($"Could not parse log varibales: " + e);
            return string.Empty;
        }
    }
}
