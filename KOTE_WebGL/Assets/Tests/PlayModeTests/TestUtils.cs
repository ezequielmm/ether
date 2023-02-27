using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

public static class TestUtils
{
    public static object CallUnityPrivateMethod(this object o, string methodName, params object[] args)
    {
        switch (methodName)
        {
            // only call the method if it's one of the Unity callback methods that require user input
            case "OnMouseEnter":
            case "OnMouseExit":
                MethodInfo mi = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    return mi.Invoke(o, args);
                }

                break;
        }

        return null;
    }

    public static string BuildTestSwsmData(string messageType, string action)
    {
        return "{\"data\":{\"message_type\":\"" + messageType + "\",\"action\":\"" + action + "\",\"data\":[]}}";
    }

    public static string BuildTestSWSMErrorData(string messageType, string action)
    {
        return "{\"data\":{\"message_type\":\""+ messageType +"\",\"action\":\""+action+"\",\"data\":{\"data\":\"Test error\"}}}";
    }

    public static string BuildTestPlayerStateData(string messageType, string action)
    {
        SWSM_PlayerState stateData = new SWSM_PlayerState
        {
            data = new PlayerStateData
            {
                data = new PlayerStateData.Data
                {
                    expeditionId = "test",
                    playerState = new PlayerData
                    {
                        cards = new List<Card>(),
                        defense = 0,
                        hpCurrent = 1,
                        hpMax = 1,
                        characterClass = "test"
                    }
                }
            }
        };

        SWSM_TestPlayerState testState = new SWSM_TestPlayerState
        {
            data = new SWSM_TestPlayerState.SWSM_DataPayload
            {
                action = "activate_portal",
                message_type = "player_state_update",
                data = stateData
            }
        };
        return JsonConvert.SerializeObject(testState);
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
        
        SWSM_TestCardPiles baseJson = new SWSM_TestCardPiles()
        {
            data = new SWSM_TestCardPiles.SWSM_DataPayload
            {
                message_type = messageType,
                action = action,
                data = returnPiles
            }
        };
        return JsonConvert.SerializeObject(baseJson);
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

        return JsonConvert.SerializeObject(intentData);
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

        return JsonConvert.SerializeObject(statusData);
    }

    public static string BuildTestCardMoveData(string messageType, string action, int numberOfMoves)
    {
        // had to do this with strings instead of objects because JsonConvert.SerializeObject didn't like this one
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
        string data = JsonConvert.SerializeObject(turnData.data.data);

        SWSM_TestBase baseJson = new SWSM_TestBase
        {
            data = new SWSM_TestBase.SWSM_DataPayload
            {
                action = action,
                data = data,
                message_type = messageType
            }
        };
        return JsonConvert.SerializeObject(baseJson);
    }

    public static string BuildTestRewardsData(string messageType, string action)
    {
        SWSM_RewardsData rewardsData = new SWSM_RewardsData
        {
            data = new SWSM_RewardsData.Data
            {
                data = new SWSM_RewardsData.Data.RewardsData
                {
                    rewards = new List<RewardItemData>
                    {
                        new()
                        {
                            amount = 1,
                            potion = new PotionData(),
                            type = "potion",
                            id = "test",
                            taken = false
                        }
                    }
                }
            }
        };

        SWSM_TestRewards baseJson = new SWSM_TestRewards
        {
            data = new SWSM_TestRewards.SWSM_DataPayload
            {
                action = action,
                data = rewardsData,
                message_type = messageType
            }
        };
        return JsonConvert.SerializeObject(baseJson);
    }

    public static string BuildTestPlayersData(string messageType, string action)
    {
        SWSM_Players rewardsData = new SWSM_Players
        {
            data = new PlayersData
            {
                data = new PlayerData
                {
                    playerName = "test"
                }
            }
        };

        SWSM_TestPlayers baseJson = new SWSM_TestPlayers
        {
            data = new SWSM_TestPlayers.SWSM_DataPayload
            {
                action = action,
                data = rewardsData,
                message_type = messageType
            }
        };
        return JsonConvert.SerializeObject(baseJson);
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

        return JsonConvert.SerializeObject(actionData);
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
                act = 0,
                id = i,
                status = "completed",
                step = i,
                type = "combat",
                subType = "combat_standard",
                title = "Node Test"
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
                        type = "royal_house",
                        title = "royal house"
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
                        type = "portal",
                        title = "portal"
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
                data = JsonConvert.SerializeObject(deckData)
            }
        };
        return JsonConvert.SerializeObject(testData);
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

    public class SWSM_TestRewards
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public SWSM_RewardsData data;
        }
    }

    public class SWSM_TestPlayers
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public SWSM_Players data;
        }
    }

    public class SWSM_TestPlayerState
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public SWSM_PlayerState data;
        }
    }

    public class SWSM_TestErrorData
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public SWSM_ErrorData data;
        }
    }
    
    public class SWSM_TestCardPiles
    {
        public SWSM_DataPayload data;

        [Serializable]
        public class SWSM_DataPayload
        {
            public string message_type;
            public string action;
            public SWSM_CardsPiles data;
        }
    }
}