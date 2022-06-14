using UnityEngine;

public static class GameSettings 
{   
    //MAP
    public const float MAP_SPRITE_ELEMENTS_Z = -20f;
    public const float MAP_SPRITE_NODE_X_OFFSET = 3.5f;
    public const float MAP_SPRITE_NODE_X_OFFSET_RH = 8f;
    public const string MAP_ELEMENTS_SORTING_LAYER_NAME = "MapElements";
    public const int MAP_LINE_RENDERER_SORTING_LAYER_ORDER = 50;
    public const float MAP_SCROLL_SPEED = 2f;
    public const float MAP_DURATION_TO_SCROLLBACK_TO_PLAYER_ICON = 2;
    public const float MAP_SCROLL_ANIMATION_DURATION = 5f;
    public const float MAP_REVEAL_ANIMATION_SPEED = 0.1f;
    public static float DOUBLE_CLICK_TIME_DELTA = 0.5f;
    public static float PORTAL_ACTIVATION_ANIMATION_TIME = 2;

    //HAND OF CARDS
    public const float HAND_CARD_SPRITE_Z = -12f;
    public const float HAND_CARD_MAX_XX_DRAG_DELTA = 5f;
    public static Vector3 HAND_CARDS_GENERATION_POINT = new Vector3(-7, -5, -9);
    



    /*//////////////  
    ///Debug
    //*/
    public const bool DEBUG_MODE_ON = true;

    
}
