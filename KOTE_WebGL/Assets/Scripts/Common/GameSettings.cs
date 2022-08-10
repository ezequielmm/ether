using UnityEngine;

public static class GameSettings 
{   
    //MAP
    public const float MAP_SPRITE_ELEMENTS_Z = -20f;
    public const float MAP_SPRITE_NODE_X_OFFSET = 3.5f;
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
    /// <summary>
    /// Negitive to go to the left, positive for the right. Between -1 and 1. -1 is left edge, 1 is right edge, and 0 is center.
    /// This affects where the knight is attepted to be put on the screen when focused.
    /// </summary>
    public const float KNIGHT_SCREEN_POSITION_ON_CENTER = -0.5f;

    //HAND OF CARDS
    public const float HAND_CARD_GAP = 2.2f;
    public const float HAND_CARD_SPRITE_Z = -12f;
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

    // COMBAT
    public const float COMBAT_ANIMATION_DELAY = 0.5f;
    public static float INTENT_TOOLTIP_SPEED = 0.2f;
    public static float INTENT_FADE_SPEED = 0.2f;
    public static float STATUS_TOOLTIP_SPEED = 0.2f;
    public static float STATUS_FADE_SPEED = 0.2f;
    public static float VICTORY_LABEL_ANIMATION_DELAY = 1f;


    /*//////////////  
    ///Debug
    //*/
    public const bool DEBUG_MODE_ON = true;

    
}
