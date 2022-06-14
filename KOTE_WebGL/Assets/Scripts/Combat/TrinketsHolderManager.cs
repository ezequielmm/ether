using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinketsHolderManager : MonoBehaviour
{
    public GameObject trinketsContainer;

    public TrinketItemManager trinketItem;

    // we need this to set the parent of the created trinkets
    public GameObject HorizontalGroup;

    // Start is called before the first frame update
    private void Start()
    {
        // we might need to change this once we get trinkets properly implemented, idk
        trinketsContainer.SetActive(false);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(DisplayRandomTrinkets);
        DisplayRandomTrinkets(null);
    }

    // when we get a player status update, that tells us to update the trinkets
    private void UpdateTrinketHolder(PlayerStateData playerState)
    {
        string[] activeTrinkets = playerState.data.player_state.trinkets;
        // currently we're picking trinkets at random, that will change once we do more with them
        if (activeTrinkets != null && activeTrinkets.Length > 0)
        {
            for (int i = 0; i < activeTrinkets.Length; i++)
            {
                TrinketItemManager trinket = Instantiate(trinketItem, HorizontalGroup.transform);
                trinket.Populate("Trinket" + Random.Range(1, 5), "Common");
            }
            trinketsContainer.SetActive(true);
        }
   
    }
     
    // --------- TEMP CODE TO BE DELETED WHEN WE RECEIVE TRINKET DATA -------------------------
    private void DisplayRandomTrinkets(PlayerStateData playerState)
    {
        for (int i = 0; i < 6; i++)
        {
            TrinketItemManager trinket = Instantiate(trinketItem, HorizontalGroup.transform);
            trinket.Populate("Trinket" + Random.Range(1, 5), "Common");
        }
        trinketsContainer.SetActive(true);
    }
    // ---------- END TEMP CODE ----------------------------------------------------------------
    
}