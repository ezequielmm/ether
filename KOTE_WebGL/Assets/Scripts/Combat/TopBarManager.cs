using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopBarManager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text classText;
    public TMP_Text healthText;
    public TMP_Text coinsText;

    private void Start()
    {
        SetTextValues(GameManager.Instance.currentProfile.data.name, GameManager.Instance.currentClass,
            GameManager.Instance.currentHealth, GameManager.Instance.currentProfile.data.coins);
    }

    public void SetTextValues(string nameText, string classText, int health, int coins)
    {
        this.nameText.text = nameText;
        this.classText.text = classText;
        healthText.text = $"{health} health";
        coinsText.text = $"{coins} coins";
    }

    public void SetNameText(string nameText)
    {
        this.nameText.text = nameText;
    }

    public void SetClassText(string classText)
    {
        this.classText.text = classText;
    }

    public void SetHealthText(int health)
    {
        healthText.text = health.ToString();
    }

    public void SetCoinsText(int coins)
    {
        coinsText.text = coins.ToString();
    }
}