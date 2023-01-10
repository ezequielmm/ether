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
    public static float HAND_CARD_Y_CURVE = 0.25f;
    public static Vector3 HAND_CARDS_GENERATION_POINT = new Vector3(-7, -5, -9);
    public static float CARD_SFX_MIN_RATE = 0.1f; // Time in seconds between SFX of cards
    public static float CARD_DRAW_SHOW_TIME = 0.5f;
    public static float EXHAUST_EFFECT_DURATION = 0.6f;

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

    // Text Effects
    public static Color DEFAUT_TEXTEFFECT_COLOR = Color.green;
    public const float DEFAUT_TEXTEFFECT_RISE_HEIGHT = 1f;
    public static Vector2 DEFAUT_TEXTEFFECT_RISE_SPEED = new Vector2(1.7f, 2.3f);
    public const float DEFAUT_TEXTEFFECT_FADE_TIME = 0.8f;
    public const float DEFAUT_TEXTEFFECT_X_SPREAD = 1f;
    public const int DEFAUT_TEXTEFFECT_POOL_SIZE = 10;

    // UI
    public static float PANEL_SCROLL_SPEED = 30f;

    // Player Skin Defaults
    public static TraitSprite[] DEFAULT_SKIN_DATA = new[]
    {
        new TraitSprite{skinName ="Boots/Boots_Medici",
        traitType = nameof(TraitTypes.Boots)
        },
        new TraitSprite{skinName ="Gauntlet/Gauntlet_Medici",
        traitType = nameof(TraitTypes.Gauntlet)
        },
        new TraitSprite{skinName ="Weapon/Weapon_Medici",
        traitType = nameof(TraitTypes.Weapon)
        },
        new TraitSprite{skinName ="Padding/Padding_Brown",
        traitType = nameof(TraitTypes.Padding)
        },
        new TraitSprite{skinName ="Helmet/Helmet_Medici",
        traitType = nameof(TraitTypes.Helmet)
        },
        new TraitSprite{skinName ="Shield/Shield_Medici",
        traitType = nameof(TraitTypes.Shield)
        },
        new TraitSprite{skinName ="Breastplate/Breastplate_Medici",
        traitType = nameof(TraitTypes.Breastplate)
        },
        new TraitSprite{skinName ="Pauldrons/Pauldrons_Medici",
        traitType = nameof(TraitTypes.Pauldrons)
        },
        new TraitSprite{skinName ="Legguard/Legguard_Medici",
        traitType = nameof(TraitTypes.Legguard)
        }
    };

/*//////////////  
///Debug
//*/
    public const bool DEBUG_MODE_ON = false;

    
}
