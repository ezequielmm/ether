using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    //MAP
    public const float COMPLETED_NODE_SCALE = 0.9f;
    public const bool COLOR_UNAVAILABLE_MAP_NODES = true;
    public const float MAP_SPRITE_ELEMENTS_Z = -20f;
    public const float MAP_SPRITE_NODE_X_OFFSET = 4.5f;
    public const float MAP_SPRITE_NODE_X_OFFSET_RH = 8f;
    public const string MAP_ELEMENTS_SORTING_LAYER_NAME = "MapElements";
    public const int MAP_LINE_RENDERER_SORTING_LAYER_ORDER = 50;
    public const float MAP_SCROLL_SPEED = 2.5f; // how fast the screen scrolls on a click
    public const float MAP_SCROLL_BUTTON_TIME = 0.6f; // How long the screen scrolls for on a click
    public const float MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON = 2; // For continues
    public const float MAP_SCROLL_ANIMATION_DURATION = 5f; // For reveals
    public const float MAP_SCROLL_HOLD_DELAY_DURATION = 0f; // Button press before constant scroll
    public const float MAP_SCROLL_SPEED_CUTOFF = 0.01f; // Lowend cutoff when ending a scroll
    public const float MAP_REVEAL_ANIMATION_SPEED = 0.1f;
    public const float MAP_LEFT_EDGE_MULTIPLIER = 1f;
    public const float MAP_RIGHT_EDGE_MULTIPLIER = 0.3f;
    public static float DOUBLE_CLICK_TIME_DELTA = 0.5f;
    public static float PORTAL_ACTIVATION_ANIMATION_TIME = 2;
    public static float MAP_STRETCH_LIMIT = 2f;
    public const float ACTIVE_NODE_PULSE_TIME = 1f;
    public const bool MAP_AUTO_SCROLL_ACTIVE = false;
    public const bool SHOW_MAP_REVEAL_ON_PORTAL = false;

    /// <summary>
    /// Negitive to go to the left, positive for the right. Between -1 and 1. -1 is left edge, 1 is right edge, and 0 is center.
    /// This affects where the knight is attepted to be put on the screen when focused.
    /// </summary>
    public const float KNIGHT_SCREEN_POSITION_ON_CENTER = -0.5f;

    //HAND OF CARDS
    public const float HAND_CARD_GAP = 2.2f;
    public const float HAND_CARD_SPRITE_Z = -20f;
    public const float HAND_CARD_SPRITE_Z_INTERVAL = 0.5f;
    public static float HAND_CARD_SHOW_UP_Z => HAND_CARD_SPRITE_Z - (HAND_CARD_SPRITE_Z_INTERVAL * 20);
    public const float HAND_CARD_SHOW_UP_SCALE = 1.25f;
    public const float HAND_CARD_SHOW_UP_TIME = 0.5f;
    public const float HAND_CARD_RESET_POSITION_TIME = 0.2f;
    public const float HAND_CARD_MAX_XX_DRAG_DELTA = 5f;
    public static float HAND_CARD_REST_Y => (Camera.main.orthographicSize * -1) - 0.5f;
    public static float HAND_CARD_SHOW_UP_Y => HAND_CARD_REST_Y + 3f;
    public static float CARD_SPAWN_POSITION => HAND_CARD_REST_Y - 10;
    public static float HAND_CARD_Y_CURVE = 0.25f;
    public static Vector3 HAND_CARDS_GENERATION_POINT = new Vector3(-7, -5, -9);
    public static float CARD_SFX_MIN_RATE = 0.1f; // Time in seconds between SFX of cards
    public static float CARD_DRAW_SHOW_TIME = 0.5f;
    public static float EXHAUST_EFFECT_DURATION = 0.6f;
    public static float SHOW_NEW_CARD_DURATION = 1;
    public static float CARD_PLAYED_TIMEOUT_DELAY = 0.5f;

    // COMBAT
    public const float COMBAT_ANIMATION_DELAY = 0.5f;
    public static float INTENT_TOOLTIP_SPEED = 0.2f;
    public static float INTENT_FADE_SPEED = 0.2f;
    public static float STATUS_TOOLTIP_SPEED = 0.2f;
    public static float STATUS_FADE_SPEED = 0.2f;
    public static float VICTORY_LABEL_ANIMATION_DELAY = 1f;
    public static float INTENT_MAX_HEIGHT = 3.2f;
    public static float INTENT_HEIGHT = 1.1f;
    public static float HEALTH_HEIGHT = 0.9f;
    public static bool SHOW_PLAYER_INJURED_IDLE = false;
    public static float END_WAIT_TIMEOUT = 5;
    public const float UI_FADEOUT_TIME = 1.0f;

    // Text Effects
    public static Color DEFAUT_TEXTEFFECT_COLOR = Color.green;
    public const float DEFAUT_TEXTEFFECT_RISE_HEIGHT = 1f;
    public static Vector2 DEFAUT_TEXTEFFECT_RISE_SPEED = new Vector2(1.7f, 2.3f);
    public const float DEFAUT_TEXTEFFECT_FADE_TIME = 0.8f;
    public const float DEFAUT_TEXTEFFECT_X_SPREAD = 1f;
    public const int DEFAUT_TEXTEFFECT_POOL_SIZE = 10;

    // UI
    public const float PANEL_SCROLL_SPEED = 30f;
    public const int ENCOUNTER_TEXT_BOX_CHARACTER_COUNT = 500;

    // Connection
    public const float UNSTABLE_CONNECTION_SECONDS = 2;
    public const float DISCONNECTED_CONNECTION_SECONDS = UNSTABLE_CONNECTION_SECONDS + 5;
    public const float MAX_TIMEOUT_SECONDS = DISCONNECTED_CONNECTION_SECONDS + 20;
    
    // Logging
    public static LogType FilterLogType = LogType.Log;

    public static int MAX_OPENSEA_CONTENT_REQUEST = 20;


#if UNITY_EDITOR
    public const string EDITOR_WALLET = "0x66956Fe08D7Bc88fe70216502fD8a6e4b7f269c5";
#endif

    public const int INITIAL_SKIN_CACHE = 3;
    // Player Skin Defaults
    public static List<TraitSprite> DEFAULT_SKIN_DATA = new List<TraitSprite>
    {
        new TraitSprite{SkinName ="Padding/Padding_Brown",
        TraitValue = "Brown",
        TraitType = Trait.Padding
        },
        new TraitSprite
        {
            SkinName = "Weapon/Weapon_Rusty_Sword",
            TraitValue =  "Rusty Sword",
            TraitType = Trait.Weapon
        },
        new TraitSprite
        {
            SkinName = "Shield/Shield_Rusty_Shield",
            TraitValue =   "Rusty Shield",
            TraitType = Trait.Shield
        },
        new TraitSprite
        {
            SkinName = "character_shadow",
            TraitValue = nameof(Trait.Shadow),
            TraitType = Trait.Shadow
        },
        new TraitSprite
        {
            SkinName = "character-nude",
            TraitValue = nameof(Trait.Base),
            TraitType = Trait.Base
        }
    };

    public static Nft DEFAULT_PLAYER = new Nft
    {
        CanPlay = true,
        Contract = NftContract.NonTokenVillager,
        Description = "The default player character",
        Image = null,
        ImageUrl = "",
        Name = "Basic Villager",
        TokenId = 1,
        Traits = new Dictionary<Trait, string>
        {
            { Trait.Padding, "Brown" },
            { Trait.Helmet, "None" },
            { Trait.Shield, "Rusty Shield" },
            { Trait.Weapon, "Rusty Sword" },
        }
    };

/*//////////////  
///Debug
//*/
    public const bool DEBUG_MODE_ON = false;

    
}
