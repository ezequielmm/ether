using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using KOTE.UI.Armory;
using UnityEngine;

/// <summary>
/// These classes are used to deserialize JSON objects coming from/to webrequests or webscokects messages
/// </summary>
/// 
[Serializable]
public class ExpeditionMapData
{
    public int seed;
    [JsonProperty("data")]public NodeDataHelper[] nodeList;
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
    public string title;
}

[Serializable]
public class MapStructure
{
    public Dictionary<int, Act> acts = new ();
}

[Serializable]
public class Act
{
    public Dictionary<int, Step> steps = new();
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
public class ExpeditionStatus
{
    [JsonProperty("hasExpedition")]
    public bool HasExpedition;
    [JsonProperty("nftId")]
    public int NftId;
    [JsonProperty("equippedGear")]
    public List<GearItemData> EquippedGear;
    [JsonProperty]
    private string tokenType;
    public NftContract TokenType => GetContractType();
    [JsonProperty("contest")] public ContestData Contest = new ContestData();

    private NftContract GetContractType()
    {
        switch (tokenType)
        {
            case "knight":
                return NftContract.Knights;
            case "villager":
                return NftContract.Villager;
            case "blessed-villager":
                return NftContract.BlessedVillager;
            case "non-token-villager":
                return NftContract.NonTokenVillager;
        }

        return NftContract.None;
    }
}

[Serializable]
public class ContestData
{
    [JsonProperty("map_id")] public string MapId;
    [JsonProperty("event_id")] public string EventId;
    [JsonProperty("available_at")] public DateTime StartTime;
    [JsonProperty("ends_at")] public DateTime SubmissionsUntilTime;
    [JsonProperty("valid_until")] public DateTime EndTime;
}

[Serializable]
public class RandomNameData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string username;
    }
}

[Serializable]
public class ProfileData
{
    [JsonProperty("userAddress")]
    public string UserAddress = null;
    [JsonProperty("displayName")]
    public string DisplayName = null;
}

[Serializable]
public class LogoutData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string message;
    }
}

[Serializable]
public class PlayerStateData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string expeditionId;
        public DateTime expeditionCreatedAt;
        public PlayerData playerState = new();
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
    [Obsolete("Int IDs are phased out.")] public int playerId;

    public string id;
    public int hpCurrent;
    public int hpMax; //current
    public int gold;
    public int energy;
    public int energyMax;
    public int defense;
    public List<Card> cards = new();
    public List<PotionData> potions = new();
    public List<Trinket> trinkets = new();
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
    public Effects properties = new();
    public List<string> keywords = new();
}

[Serializable]
public class Trinket
{
    public string id;
    public int trinketId;
    public string name;
    public string rarity;
    public string description;
    public int counter = 0;
    public Effects effects = new();
}

[Serializable]
public class PotionData
{
    public string id;
    public int potionId;
    public string name;
    public string rarity;
    public string description;
    public int cost;
    public List<Effect> effects = new();
    public bool usableOutsideCombat;
    public bool showPointer;
}

[Serializable]
public class Effects
{
    public List<Effect> effects = new();
    public List<Statuses> statuses = new();
}

[Serializable]
public class Statuses
{
    public string name;
    public Args args = new();
    public Tooltip tooltip = new();

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
    public string target;
}


[Serializable]
public class Deck
{
    public Deck()
    {
    }

    public Deck(List<Card> cards)
    {
        this.cards = cards;
    }

    public List<Card> cards = new();
}


[Serializable]
public class CardPiles
{
    public Cards data = new();
}

#region NODESTATE

[Serializable]
public class NodeStateData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public int node_id;
        public bool completed;
        public string node_type;
        public Data1 data = new();

        [Serializable]
        public class Data1
        {
            public int round;
            public int action;

            public PlayerData player = new();

            [Serializable]
            public class PlayerData
            {
                public int energy;
                public int energy_max;
                public int hand_size;

                public Cards cards = new();
            }
        }
    }
}

[Serializable]
public class Cards
{
    public List<Card> draw = new();
    public List<Card> hand = new();
    public List<Card> discard = new();
    public List<Card> exhausted = new();
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
public class PurchaseData //outgoing data
{
    public string type;
    public string targetId;
}

[Serializable]
public class CardsSelectedList
{
    public List<string> cardsToTake = new();
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
    public Data data = new();

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
    public SWSM_CommonFields data = new();

    [Serializable]
    public class SWSM_CommonFields
    {
        public string message_type;
        public string action;
    }
}

[Serializable]
public class SWSM_TreasureData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public string data;
    }
}

[Serializable]
public class SWSM_ChestResult
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public ChestResult data = new();
    }
}

[Serializable]
public class ChestResult
{
    public string isOpen;
    public List<RewardItemData> rewards = new();
    public TrappedResult trapped = new();
}

[Serializable]
public class TrappedResult
{
    public string trappedType;
    public string trappedText;
    public string monster_type;
    public int damage;
    public Card curse_card = new();
}

[Serializable]
public class SWSM_PlayerDeckData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<Card> data = new();
    }
}

[Serializable]
public class SWSM_DeckData
{
    public SWSM_Deck data = new();
}

[Serializable]
public class SWSM_Deck
{
    public DeckData data = new();
}

[Serializable]
public class DeckData
{
    public List<Card> deck = new();
}

[Serializable]
public class CardUpgrade
{
    [JsonProperty("cardIdToDelete")] public string CardIdToDelete;
    [JsonProperty("newCard")] public Card NewCard;
}

[Serializable]
public class SWSM_MapData
{
    [JsonProperty("data")]public ExpeditionMapData expeditionData = new();
}

[Serializable]
public class SWSM_NodeData
{
    public NodeStateData data = new();
}

[Serializable]
public class SWSM_IntentData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public List<EnemyIntent> data = new();
        public string action;
        public string message_type;
    }
}

[Serializable]
public class EnemyIntent
{
    public string id;
    public List<Intent> intents = new();

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
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<StatusData> data = new();
    }
}

[Serializable]
public class SWSM_CombatAction
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string message_type;
        public string action;
        public List<CombatTurnData> data = new();
    }
}

[Serializable]
public class StatusData
{
    public string targetEntity;
    public string id;
    public List<Status> statuses = new();

    [Serializable]
    public class Status
    {
        public string name;
        public int counter;
        public string description;
    }
}

[Serializable]
public class SWSM_ShowCardDialog
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public showCardData data = new();
    }
}

[Serializable]
public class showCardData
{
    public List<Card> cards = new();
    public int cardsToTake;
    public string kind;
}

[Serializable]
public class SWSM_CardUpdateData
{
    public CardUpdateData data = new();

    [Serializable]
    public class CardUpdateData
    {
        public UpdateCardData data = new();

        [Serializable]
        public class UpdateCardData
        {
            public Card card;
            public string pile;
        }
    }
}

[Serializable]
public class SWSM_ErrorData
{
    public string data;
}

[Serializable]
public class SWSM_PlayerState
{
    public PlayerStateData data = new();
}

[Serializable]
public class SWSM_EnergyArray
{
    public EnergyData data = new();

    [Serializable]
    public class EnergyData
    {
        public int[] data;
    }
}

[Serializable]
public class SWSM_CurrentStep
{
    public StepData data = new();

    [Serializable]
    public class StepData
    {
        public string message_type;
        public string action;

        //public string data;
        public CurrentStep data = new();
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
    public CardPiles data = new();
}

[Serializable]
public class SWSM_Enemies
{
    public EnemiesData data = new();
}

[Serializable]
public class EnemiesData
{
    public List<EnemyData> data = new();
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
    [Obsolete("Int IDs are phased out.")] public int enemyId;

    public int defense;
    public int hpCurrent; //current
    public int hpMax;
    public string type;
    public string category;
    public string size;
}

public class SWSM_Players
{
    public PlayersData data = new();
}

[Serializable]
public class PlayersData
{
    public PlayerData data = new(); //future array when multiplayer
}


public class SWSM_CardMove
{
    public Data data = new();

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
    public Card card = new();
}

[Serializable]
public class SWSM_CardAdd
{
    public Data data;

    [Serializable]
    public class Data
    {
        public List<AddCardData> data = new();
    }
}

[Serializable]
public class AddCardData
{
    public string destination;
    public string id;
    public Card card = new();
}

[Serializable]
public class SWSM_ChangeTurn
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public string data;
    }
}

[Serializable]
public class SWSM_RewardsData
{
    public Data data = new();

    [Serializable]
    public class Data
    {
        public RewardsData data = new();

        [Serializable]
        public class RewardsData
        {
            public List<RewardItemData> rewards = new();
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
    public PotionData potion = new();
    public Card card = new();
    public Trinket trinket = new();
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
    public HealData data = new();

    [Serializable]
    public class HealData
    {
        public HealAmount data = new();

        [Serializable]
        public class HealAmount
        {
            public int healed;
        }
    }
}

[Serializable]
public class SWSM_SelectTrinketData
{
    public SelectTrinketData data = new();

    [Serializable]
    public class SelectTrinketData
    {
        public TrinketData data = new();
    }
}

[Serializable]
public class TrinketData
{
    public List<Trinket> trinkets = new();
}

[Serializable]
public class TraitValue
{
    public string trait_type;
    public string value;
}

[Serializable]
public class WalletKnightIds
{
    public int[] data;
}

[Serializable]
public class WhitelistResponse
{
    public WhitelistData data = new();

    [Serializable]
    public class WhitelistData
    {
        public bool isValid;
    }
}

[Serializable]
public class BugReportData
{
    public string reportId;
    public string environment;
    public const string service = "Frontend";
    public string clientId;
    public string account;
    public int knightId;
    public string expeditionId;
    public string userDescription;
    public string userTitle;
    public string screenshot;
    public string frontendVersion;
    public string backendVersion = "???";
    public List<ServerCommunicationLogger.ServerCommunicationLog> messageLog = new();
}

[Serializable]
public class ExpeditionStartSendData
{
    public string tokenType;
    public int nftId;
    public List<GearItemData> equippedGear;
    public string walletId;
    public string contractId;
}

[Serializable]
public class ExpeditionStartData
{
    public bool expeditionCreated;
    public string reason;
}

[Serializable]
public class TrinketTriggeredData
{
    public TrinketInfo data;

    [Serializable]
    public class TrinketInfo
    {
        [JsonProperty("data")] public Trinket trinket;
    }
}