using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class HealthBarManager : MonoBehaviour
{
    [Tooltip("Set whether the health bar is attached to the player or an enemy")]
    public EntityType combatEntityAttachedTo;
    
    //the various UI elements the manager controls
    public Slider healthBar;
    public GameObject blockGauge;
    public TMP_Text blockText;

    // keep track of the value locally so we can check when to hide it
    private int blockAmount;

    private void Start()
    {
        if (combatEntityAttachedTo == EntityType.Player)
        {
            GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(GetPlayerUpdate);
        }
        else if (combatEntityAttachedTo == EntityType.Enemy)
        {
            //TODO add a listener to whatever event tracks when the enemy status is updated
        }
    }

    private void GetPlayerUpdate(PlayerStateData playerData)
    {
        SetCurrentHealth(playerData.data.player_state.hp_current, playerData.data.player_state.hp_max);
    }

    // use this to outright set the amount of health the player has left
    private void SetCurrentHealth(int curHealth, int maxHealth)
    {
        float healthPercentage = (float)curHealth / maxHealth;
        if (healthPercentage < 0) healthPercentage = 0;
        if (healthPercentage > 1) healthPercentage = 1;
        healthBar.value = healthPercentage;
    }


    private void SetCurrentBlock(int blockAmount)
    {
        this.blockAmount = blockAmount;
        if (this.blockAmount <= 0)
        {
            this.blockAmount = 0;
            blockText.text = this.blockAmount.ToString();
            blockGauge.SetActive(false);
            return;
        }
        blockGauge.SetActive(true);
    }

}
