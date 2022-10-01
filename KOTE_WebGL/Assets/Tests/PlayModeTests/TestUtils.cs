using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TestUtils
{
    public static string BuildTestSwsmData(string messageType, string action)
    {
        return "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action + "\",\"data\":[]}}";
    }

    public static string BuildTestPlayerStateData(string messageType, string action)
    {
        string data = "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action +
                      "\",\"data\":[{\"playerName\": \"test\", \"characterClass\": \"test\", \"hpCurrent\": 23, \"hpMax\": 30, \"gold\": 10, \"energy\": 3, \"energyMax\":3, \"defense\": 2, \"cards\":[]}]}}";
        return data;
    }

    public static string BuildTestGenericEnergyData(string messageType, string action)
    {
        return "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action + "\",\"data\":[1, 2]}}";
    }

    public static string BuildTestGenericCardPilesData(string messageType, string action)
    {
        SWSM_CardsPiles returnPiles = new SWSM_CardsPiles
        {
            data = new CardPiles
            {
                data = new Cards
                {
                    discard = new List<Card>(),
                    draw = new List<Card>(),
                    energy = 1,
                    energy_max = 1,
                    exhausted = new List<Card>(),
                    hand = new List<Card>()
                }
            }
        };
        string data = JsonUtility.ToJson(returnPiles.data.data);
        SWSM_TestBase baseJson = new SWSM_TestBase()
        {
            data = new SWSM_TestBase.SWSM_DataPayload
            {
                message_type = messageType,
                action = action,
                data = data
            }
        };
        return JsonUtility.ToJson(baseJson);
    }

    public static string BuildTestEnemyIntentData(string messageType, string action, int numberOfIntents)
    {
        SWSM_IntentData intentData = new SWSM_IntentData
        {
            data = new SWSM_IntentData.Data
            {
                action = action,
                data = new List<EnemyIntent>(),
                message_type = messageType
            }
        };
        for (int i = 0; i < numberOfIntents; i++)
        {
            intentData.data.data.Add(new EnemyIntent
            {
                id = "",
                intents = new List<EnemyIntent.Intent>
                    { new EnemyIntent.Intent { description = "", type = "", value = 2 } }
            });
        }

        return JsonUtility.ToJson(intentData);
    }

    public static string BuildTestStatusData(string messageType, string action, int numberOfStatuses)
    {
        SWSM_StatusData statusData = new SWSM_StatusData
        {
            data = new SWSM_StatusData.Data
            {
                action = action,
                data = new List<StatusData>(),
                message_type = messageType
            }
        };
        for (int i = 0; i < numberOfStatuses; i++)
        {
            statusData.data.data.Add(new StatusData
            {
                id = "",
                statuses = new List<StatusData.Status>
                    { new StatusData.Status { counter = 2, description = "", name = "" } }
            });
        }

        return JsonUtility.ToJson(statusData);
    }

    public static string BuildTestCardMoveData(string messageType, string action, int numberOfMoves)
    {
        // had to do this with strings instead of objects because JsonUtility.ToJson didn't like this one
        string data = "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action +
                      "\",\"data\":[";
        for (int i = 0; i < numberOfMoves; i++)
        {
            data += "{\"source\":\"hand\",\"destination\":\"discard\",\"id\":\"eb7cc08c-af97-4261-bbad-b0e11aacafd8\"}";
            if (i != numberOfMoves - 1) data += ",";
        }

        data += "]}}";
        return data;
    }

    public static string BuildTestChangeTurnData(string messageType, string action)
    {
        SWSM_ChangeTurn turnData = new SWSM_ChangeTurn
        {
            data = new SWSM_ChangeTurn.Data
            {
                data = ""
            }
        };
        string data = JsonUtility.ToJson(turnData.data.data);

        SWSM_TestBase baseJson = new SWSM_TestBase
        {
            data = new SWSM_TestBase.SWSM_DataPayload
            {
                action = action,
                data = data,
                message_type = messageType
            }
        };
        return JsonUtility.ToJson(baseJson);
    }

    [Serializable]
    public class SWSM_TestBase
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public string data;
        }
    }
}