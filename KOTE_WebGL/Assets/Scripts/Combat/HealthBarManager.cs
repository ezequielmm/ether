using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarManager : MonoBehaviour
{
    public Slider HealthBar;

    // use this to outright set the amount of health the player has left
    private void SetCurrentHealth(int curHealth, int maxHealth)
    {
        float healthPercentage = (float)curHealth / maxHealth;
        if (healthPercentage < 0) healthPercentage = 0;
        if (healthPercentage > 1) healthPercentage = 1;
        HealthBar.value = healthPercentage;
    }
    
    // use these if the health bar itself is going to keep track of the amount of damage taken
    private void TakeDamage(int damage)
    {
        float damageTaken = damage / 10.0f;
        HealthBar.value -= damageTaken;
        if (HealthBar.value < 0) HealthBar.value = 0;
    }

    private void Heal(int healAmount)
    {
        float damageHealed = healAmount / 10.0f;
        HealthBar.value += damageHealed;
        if (HealthBar.value > 1) HealthBar.value = 1;
    }
}
