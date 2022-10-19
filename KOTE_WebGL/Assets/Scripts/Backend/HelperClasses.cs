using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// These classes are used to deserialize JSON objects coming from/to webrequests or webscokects messages
/// </summary>
/// 
[Serializable]
public class ExpeditionMapData
{
    public int seed;
    public NodeDataHelper[] data;
}

[Serializable]
public class NodeDataHelper
{
    public int act;
    public int step;
    public int id;
    public string type;
    public string subType;
    public string status;
    public int[] exits;
    public int[] enter;
}

[Serializable]
public class MapStructure
{
    public List<Act> acts = new List<Act>();
}

[Serializable]
public class Act
{
    public List<Step> steps = new List<Step>();
}

[Serializable]
public class TargetProfile
{
    /// <summary>
    /// If the Players can be targeted
    /// </summary>
    public bool player;

    /// <summary>
    /// If the Enemies can be targeted
    /// </summary>
    public bool enemy;

    /// <summary>
    /// This is true if the target can not be specified. If so, when player or enemy
    /// is true, that means that who may get targeted at random or as a whole.
    /// </summary>
    public bool notSpecified;

    /// <summary>
    /// A List of specific entities that can be targeted.
    /// </summary>
    public List<string> specificEntities = new List<string>();
}

[Serializable]
public class Step
{
    public List<NodeDataHelper> nodesData = new List<NodeDataHelper>();
}

[Serializable]
public class Tooltip
{
    public string title;
    public string description;
}


[Serializable]
public class ExpeditionStatusData
{
    public Data data;

    public bool GetHasExpedition()
    {
        return this.data.hasExpedition == "true";
    }

    [Serializable]
    public class Data
    {
        public string hasExpedition;
    }
}

public class ExpeditionRequestData
{
    public Data data;

    public bool GetExpeditionStarted()
    {
        return this.data.expeditionCreated == "true";
    }

    [Serializable]
    public class Data
    {
        public string expeditionCreated;
    }
}

[Serializable]
public class RandomNameData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string username;
    }
}

[Serializable]
public class RegisterData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string token;
        public string name;
    }
}

[Serializable]
public class LoginData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string token;
    }
}

[Serializable]
public class ProfileData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string id;
        public string name;
        public string email;
        public List<string> wallets;
        public int coins;
        public int fief;
        public int experience;
        public int level;
        public int act;
        public ActMap act_map;

        [Serializable]
        public class ActMap
        {
            public string id;
            public string currentNode;
        }
    }
}

[Serializable]
public class LogoutData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message;
    }
}

[Serializable]
public class PlayerStateData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public PlayerData playerState;
    }
}

[Serializable]
public class PlayerData
{
    public string playerName;
    public string characterClass;

    /// <summary>
    /// Index of Player
    /// </summary>
    [Obsolete("Int IDs will be phased out")]
    public int playerId { get; set; } = 1; // This will be static for now. We'll need this when we have multiple players

    public string id;
    public int hpCurrent;
    public int hpMax; //current
    public int gold;
    public int energy;
    public int energyMax;
    public int defense;
    public List<Card> cards;
    public List<Potion> potions;
}

[Serializable]
public class Card
{
    public string name;

    public string id;
    public int cardId;
    public string description;
    public string rarity;
    public int energy;
    public string cardType;
    public bool isUpgraded;

    public string pool;
    public bool showPointer;
    public Effects properties;
    public List<string> keywords;
}

[Serializable]
public class Potion
{
    public string id;
    public int potionId;
    public string name;
    public string rarity;
    public string description;
    public int cost;
    public List<Effect> effects;
    public bool usableOutsideCombat;
    public bool showPointer;
}

[Serializable]
public class Effects
{
    public List<Effect> effects;
    public List<Statuses> statuses;
}

[Serializable]
public class Statuses
{
    public string name;
    public Args args;
    public Tooltip tooltip;

    [Serializable]
    public class Args
    {
        public int value;
        public string attachTo;
        public string description;
    }
}

[Serializable]
public class Effect
{
    public string effect;
    public string target;
    public EffectArgs args;
}


[Serializable]
public class EffectArgs
{
    public int value;
}

[Serializable]
public class Deck
{
    public List<Card> cards;
}


[Serializable]
public class CardPiles
{
    public Cards data;
}

#region NODESTATE

[Serializable]
public class NodeStateData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public int node_id;
        public bool completed;
        public string node_type;
        public Data1 data;

        [Serializable]
        public class Data1
        {
            public int round;
            public int action;

            public PlayerData player;

            [Serializable]
            public class PlayerData
            {
                public int energy;
                public int energy_max;
                public int hand_size;

                public Cards cards;
            }
        }
    }
}

[Serializable]
public class Cards
{
    public List<Card> draw;
    public List<Card> hand;
    public List<Card> discard;
    public List<Card> exhausted;
    public int energy;
    public int energy_max;
}

#endregion

[Serializable]
public class CardPlayedData //outgoing data
{
    public string cardId;
    public string targetId;
}

[Serializable]
public class PotionUsedData
{
    public string potionId;
    public string targetId;
}

[Serializable]
public class Errordata
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message;
    }
}

//SWSM
[Serializable]
public class SWSM_Base
{
    public SWSM_CommonFields data;

    [Serializable]
    public class SWSM_CommonFields
    {
        public string message_type;
        public string action;
    }
}

[Serializable]
public class SWSM_PlayerDeckData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<Card> data;
    }
}

[Serializable]
public class SWSM_DeckData
{
    public SWSM_Deck data;
}

[Serializable]
public class SWSM_Deck
{
    public DeckData data;
}

[Serializable]
public class DeckData
{
    public List<Card> deck;
}

[Serializable]
public class SWSM_ConfirmUpgrade
{
    public SWSM_UpgradeData data;

    [Serializable]
    public class SWSM_UpgradeData
    {
        public UpgradeData data;

        [Serializable]
        public class UpgradeData
        {
            public string cardIdToDelete;
            public Card newCard;
        }
    }
}

[Serializable]
public class SWSM_MapData
{
    public ExpeditionMapData data;
}

[Serializable]
public class SWSM_NodeData
{
    public NodeStateData data;
}

[Serializable]
public class SWSM_IntentData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public List<EnemyIntent> data;
        public string action;
        public string message_type;
    }
}

[Serializable]
public class EnemyIntent
{
    public string id;
    public List<Intent> intents;

    [Serializable]
    public class Intent
    {
        public int value;
        public string description;
        public string type;
    }
}

[Serializable]
public class SWSM_StatusData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<StatusData> data;
    }
}

[Serializable]
public class SWSM_CombatAction
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<CombatTurnData> data;
    }
}

[Serializable]
public class StatusData
{
    public string targetEntity;
    public string id;
    public List<Status> statuses;

    [Serializable]
    public class Status
    {
        public string name;
        public int counter;
        public string description;
    }
}


[Serializable]
public class SWSM_ErrorData
{
    public String data;
}

public class SWSM_PlayerState
{
    public PlayerStateData data;
}

[Serializable]
public class SWSM_EnergyArray
{
    public EnergyData data;

    [Serializable]
    public class EnergyData
    {
        public int[] data;
    }
}

[Serializable]
public class SWSM_CurrentStep
{
    public StepData data;

    [Serializable]
    public class StepData
    {
        public string message_type;
        public string action;

        //public string data;
        public CurrentStep data;
    }
}

[Serializable]
public class CurrentStep
{
    public int act;
    public int step;
}

[Serializable]
public class SWSM_CardsPiles
{
    public CardPiles data;
}

[Serializable]
public class SWSM_Enemies
{
    public EnemiesData data;
}

[Serializable]
public class EnemiesData
{
    public List<EnemyData> data;
}

[Serializable]
public class EnemyData
{
    /// <summary>
    /// GUID of enemy
    /// </summary>
    public string id;

    public string name;

    /// <summary>
    /// Index of enemy
    /// </summary>
    [Obsolete("Int IDs will be phased out")]
    public int enemyId;

    public int defense;
    public int hpCurrent; //current
    public int hpMax;
    public string type;
    public string category;
    public string size;
}

public class SWSM_Players
{
    public PlayersData data;
}

[Serializable]
public class PlayersData
{
    public PlayerData data; //future array when multiplayer
}


public class SWSM_CardMove
{
    public Data data;

    [Serializable]
    public class Data
    {
        public CardToMoveData[] data;
    }
}

[Serializable]
public class CardToMoveData
{
    public string source;
    public string destination;
    public string id;
}

public class SWSM_ChangeTurn
{
    public Data data;

    [Serializable]
    public class Data
    {
        public string data;
    }
}

public class SWSM_RewardsData
{
    public Data data;

    [Serializable]
    public class Data
    {
        public RewardsData data;

        [Serializable]
        public class RewardsData
        {
            public List<RewardItemData> rewards;
        }
    }
}

[Serializable]
public class RewardItemData
{
    public string id;
    public string type;
    public int amount;
    public bool taken;
    public RewardPotion potion;
    public Card card;
}

[Serializable]
public class RewardPotion
{
    public int potionId;
    public string name;
    public string description;
}

[Serializable]
public class SWSM_HealData
{
    public HealData data;

    [Serializable]
    public class HealData
    {
        public HealAmount data;

        [Serializable]
        public class HealAmount
        {
            public int healed;
        }
    }
}