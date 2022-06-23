using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TrinketsHolderManager : MonoBehaviour
{
    public GameObject trinketsContainer;

    public TrinketItemManager trinketItem;

    // we need this to set the parent of the created trinkets
    [FormerlySerializedAs("HorizontalGroup")] public GameObject GridLayout;

    // Start is called before the first frame update
    private void Start()
    {
        // we might need to change this once we get trinkets properly implemented, idk
        trinketsContainer.SetActive(false);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(UpdateTrinketHolder);
        DisplayRandomTrinkets();
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
                TrinketItemManager trinket = Instantiate(trinketItem, GridLayout.transform);
                trinket.Populate("Trinket" + Random.Range(1, 5), "Common");
            }
            trinketsContainer.SetActive(true);
        }
   
    }
     
    // --------- TEMP CODE TO BE DELETED WHEN WE RECEIVE TRINKET DATA -------------------------
    private void DisplayRandomTrinkets()
    {
        for (int i = 0; i < Random.Range(3, 10); i++)
        {
            TrinketItemManager trinket = Instantiate(trinketItem, GridLayout.transform);
            trinket.Populate("Trinket" + Random.Range(1, 5), "Common");
        }
        trinketsContainer.SetActive(true);
    }
    // ---------- END TEMP CODE ----------------------------------------------------------------
    
}