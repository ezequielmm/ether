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
[Serializable]
public enum GameStatuses
{
    Combat,
    Map,
    Encounter,
    Merchant,
    RoyalHouse,
    RewardsPanel,
    GameOver
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

public enum ENEMY_INTENT 
{
    unknown = 0,

    attack,
    defend,
    plot, // buff
    scheme, // debuff
    stunned // nothing
}

public enum STATUS
{
    unknown = 0,

    resolve,
    fortitude,
    distraught,
    feeble,
    fatigue,
    resist,
    spirited,
    spikes,
    dodge,
    forceField,
    regeneration,
    intercept,
    enraged,
    heraldDelayed,
    heralding,
    gritted,
    grittedPlus,
    resolveExpires,
    bolstered,
    anticipating,
    siphoning,
    turtling,
    imbued,
    doubleDown,
    praying,
    trinity,
    trinityPlus,
    enflamed,
    gifted,
    finelyEdged,
    finelyEdgedPlus,
    armoredUp,
    armoredUpPlus,
    dewDrop,
    burn,
    tasteOfBlood,
    confusion,
    confused,
    stunned,
    drained,
    summoned,
    clearheaded,
    blinded,
    blighted,
    oiled,
    trapping,
    mimic,
    moldy,
    squishy,
    combustible,
    hidden,
    reactive,
    mysteriousFigure,
    emptyFade
}

public enum EntityType
{
    Player,
    Enemy
}

[Serializable]
public enum WS_DATA_REQUEST_TYPES
{
    Energy,//done
    Health,
    Players,
    CardsPiles,//done
    Enemies,
    EnemyIntents,
    Potions,
    Trinkets,
    Statuses,
    PlayerDeck
}

[Serializable]
public enum WS_ERROR_TYPES
{
    card_unplayable,
    invalid_card,
    insufficient_energy
}

[Serializable]
public enum WS_MESSAGE_TYPES
{
    map_update,
    combat_update,
    enemy_intents,
    player_state_update,
    error,
    generic_data,
    enemy_affected,
    player_affected,
    end_turn,
    begin_turn,
    end_combat
}

[Serializable]
public enum WS_MESSAGE_ACTIONS
{
    update_energy,
    move_card,
    update_enemy,
    update_player,
    change_turn,
    create_card,
    enemies_defeated,
    players_defeated

}

[Serializable]
public enum CARDS_POSITIONS_TYPES
{
    discard,
    hand,
    exhaust,
    draw
}

[Serializable]
public enum ATTACK_EFFECT_TYPES
{
    damage,
    defense,
    health
}

[Serializable]
public enum RewardItemType
{
    cards,
    gold,
    potion,
    trinket,
    fief
}


