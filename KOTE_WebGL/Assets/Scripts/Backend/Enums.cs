using System;

public enum inGameScenes
{
    MainMenu,
    Loader,
    Expedition
}; //created so we can use the names on the enums instead of hard coding strings everytime, if a scene name is changed we can just change it here as well instead of changing at various spots

[Serializable]
public enum PileTypes
{
    Deck,
    Draw,
    Exhausted,
    Discarded
}

public enum GameStatuses
{
    Combat,
    Map,
    Encounter,
    Merchant,
    RoyalHouse
}

public enum WS_QUERY_TYPE
{
    MAP_NODE_SELECTED,
    CARD_PLAYED,
    END_OF_TURN
}

public enum NODE_TYPES
{
    undefined = 0,
    royal_house,
    portal,
    encounter,
    combat,
    camp,
    merchant,
    treasure
}

public enum NODE_SUBTYPES
{
    undefined = 0,
    royal_house_a,
    royal_house_b,
    royal_house_c,
    royal_house_d,
    portal,
    encounter,
    combat_standard,
    combat_elite,
    combat_boss,
    camp_regular,
    camp_house,
    merchant,
    treasure
}

public enum NODE_STATUS
{
    undefined = 0,
    completed,
    active,
    available,
    disabled
    
}

public enum EntityType
{
    Player,
    Enemy
}

