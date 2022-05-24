using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarManager : MonoBehaviour
{
    public Slider healthBar;
    public GameObject armorGauge;
    public TMP_Text armorText;

    // keep track of the value locally so we can check when to hide it
    private int blockAmount;

    // use this to outright set the amount of health the player has left
    private void SetCurrentHealth(int curHealth, int maxHealth)
    {
        float healthPercentage = (float)curHealth / maxHealth;
        if (healthPercentage < 0) healthPercentage = 0;
        if (healthPercentage > 1) healthPercentage = 1;
        healthBar.value = healthPercentage;
    }
    
    // use these if the health bar itself is going to keep track of the amount of damage taken
    private void TakeDamage(int damage)
    {
        float damageTaken = damage / 10.0f;
        healthBar.value -= damageTaken;
        if (healthBar.value < 0) healthBar.value = 0;
    }

    private void Heal(int healAmount)
    {
        float damageHealed = healAmount / 10.0f;
        healthBar.value += damageHealed;
        if (healthBar.value > 1) healthBar.value = 1;
    }

    // use these to add/remove block from the gauge, also handles showing the indicator
    private void AddBlock(int blockAmount)
    {
        blockAmount += blockAmount;
        armorText.text = blockAmount.ToString();
        armorGauge.SetActive(true);
    }
    
    private void RemoveBlock(int blockAmount)
    {
        blockAmount -= blockAmount;
        if (blockAmount <= 0)
        {
            armorGauge.SetActive(false);
            blockAmount = 0;
        }
        armorText.text = blockAmount.ToString();
        
    }
}
