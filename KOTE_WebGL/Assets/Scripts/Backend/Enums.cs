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
    royal_house,
    portal,
    encounter,
    combat,
    camp,
    merchant
}

public enum NODE_SUBTYPES
{
    royal_house,
    portal,
    encounter,
    combat_standard,
    combar_elite,
    combat_boss,
    camp_regular,
    camp_house,
    merchant
}

public enum NODE_STATUS
{
    completed,
    active,
    available,
    disabled
}