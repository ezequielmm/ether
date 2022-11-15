using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TrinketsHolderManager : MonoBehaviour
{
    public GameObject trinketsContainer;

    public TrinketItemManager trinketItem;
    private List<TrinketItemManager> spawnedTrinketList;

    // we need this to set the parent of the created trinkets
    [FormerlySerializedAs("HorizontalGroup")] public GameObject GridLayout;

    // Start is called before the first frame update
    private void Start()
    {
        // we might need to change this once we get trinkets properly implemented, idk
        trinketsContainer.SetActive(false);
        spawnedTrinketList = new List<TrinketItemManager>();
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(UpdateTrinketHolder);
    }

    // when we get a player status update, that tells us to update the trinkets
    private void UpdateTrinketHolder(PlayerStateData playerState)
    {
        List<Trinket> activeTrinkets = playerState.data.playerState.trinkets;
        // currently we're picking trinkets at random, that will change once we do more with them
        if (activeTrinkets != null && activeTrinkets.Count > 0)
        {
            for (int i = 0; i < activeTrinkets.Count; i++)
            {
                if (spawnedTrinketList.Exists(trinket => trinket.Id == activeTrinkets[i].id))
                {
                    continue;
                }
                TrinketItemManager trinket = Instantiate(trinketItem, GridLayout.transform);
                trinket.Populate(activeTrinkets[i]);
                spawnedTrinketList.Add(trinket);
            }
            trinketsContainer.SetActive(true);
        }
   
    }
    
}