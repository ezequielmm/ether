using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
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

    public static string BuildTestCombatQueueData(string messageType, string action, int numberOfActions)
    {
        SWSM_CombatAction actionData = new SWSM_CombatAction
        {
            data = new SWSM_CombatAction.Data
            {
                action = action,
                data = new List<CombatTurnData>(),
                message_type = messageType
            }
        };
        for (int i = 0; i < numberOfActions; i++)
        {
            actionData.data.data.Add(new CombatTurnData
            {
                attackId = new Guid(),
                delay = 1,
                originId = "",
                originType = "",
                targets = new List<CombatTurnData.Target>()
            });
        }

        return JsonUtility.ToJson(actionData);
    }

    public static SWSM_MapData GenerateTestMap(int numberOfNodes, int activeNodeNumber)
    {
        if (activeNodeNumber >= numberOfNodes) activeNodeNumber = 0;
        SWSM_MapData mapData = new SWSM_MapData
        {
            data = new ExpeditionMapData
            {
                data = new NodeDataHelper[numberOfNodes]
            }
        };

        for (int i = 0; i < numberOfNodes; i++)
        {
            NodeDataHelper nodeData = new NodeDataHelper
            {
                act = i,
                id = i,
                status = "completed",
                step = 0,
                type = "combat",
                subType = "combat_standard"
            };
            if (i != 0) nodeData.enter = new[] { i - 1 };
            else nodeData.enter = Array.Empty<int>();
            if (i != numberOfNodes - 1) nodeData.exits = new[] { i + 1 };
            else nodeData.exits = Array.Empty<int>();
            if (i == activeNodeNumber) nodeData.status = "active";
            else if (i > activeNodeNumber) nodeData.status = "disabled";
            mapData.data.data[i] = nodeData;
        }

        return mapData;
    }

    public static SWSM_MapData GenerateTestPortalMap()
    {
        SWSM_MapData mapData = new SWSM_MapData
        {
            data = new ExpeditionMapData
            {
                data = new[]
                {
                    new NodeDataHelper
                    {
                        act = 0,
                        enter = Array.Empty<int>(),
                        exits = new[] { 1 },
                        id = 0,
                        status = "completed",
                        step = 0,
                        subType = "royal_house_a",
                        type = "royal_house"
                    },
                    new NodeDataHelper
                    {
                        act = 0,
                        enter = new[] { 0 },
                        exits = Array.Empty<int>(),
                        id = 1,
                        status = "available",
                        step = 1,
                        subType = "portal",
                        type = "portal"
                    }
                }
            }
        };


        return mapData;
    }

    public static string BuildTestUpgradeableCardData()
    {
        SWSM_PlayerDeckData deckData = new SWSM_PlayerDeckData
        {
            data = new SWSM_PlayerDeckData.Data
            {
                data = new List<Card>()
            }
        };
        SWSM_TestBase testData = new SWSM_TestBase
        {
            data = new SWSM_TestBase.SWSM_DataPayload
            {
                message_type = "generic_data",
                action = "UpgradableCards",
                data = JsonUtility.ToJson(deckData)
            }
        };
        return JsonUtility.ToJson(testData);
    }

    public static string BuildTestHealData(string messageType, string action, int healAmount)
    {
        string returnString =
            "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action + "\",\"data\":{\"healed\":" +
            healAmount + "}}}";
        return returnString;
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