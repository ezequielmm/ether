using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Refactor.TopBar
{
    public class TopBarUI : MonoBehaviour
    {
        public TMP_Text nameText;
        public TMP_Text coinsText;
        public TMP_Text stageText;
        public TMP_Text healthBarText;


        [SerializeField]
        Slider healthBar;
        
        public PotionsContainer PotionsContainer;
        
        private string currentClass;
        private int currentHealth;

        private int maxHealth;

        public GameObject classIcon, className, showMapButton;
        
        public event PotionsContainer.DrinkPotionEvent OnDrinkPotion;
        public event Action<string> OnDiscardPotion; 

        public void Init(PlayerStateData playerStateData)
        {
            // this has to be set here, as it is not visible in the inspector
            nameText.maxVisibleLines = 2;
            maxHealth = playerStateData.data.playerState.hpMax;
            
            PotionsContainer.OnDrinkPotion += (potionId, targetId) => OnDrinkPotion?.Invoke(potionId, targetId);
            PotionsContainer.OnDiscardPotion += (potionId) => OnDiscardPotion?.Invoke(potionId);
            PotionsContainer.Init();
        }
        
        public void UpdatePlayerState(PlayerStateData playerStateData)
        {
            SetNameText(playerStateData.data.playerState.playerName);
            SetHealthText(playerStateData.data.playerState.hpCurrent);
            SetCoinsText(playerStateData.data.playerState.gold);
            PotionsContainer.UpdateState(playerStateData.data.playerState.potions);
        }

        public void SetNameText(string nameText)
        {
            this.nameText.text = nameText;
        }
        
        public void SetHealthText(int health)
        {
            Debug.Log("SET HEALTH " + health);
            healthBar.value = (float)(health) / maxHealth;
            healthBarText.text = $"{health}/{maxHealth}";
        }
        
        public void SetCoinsText(int coins)
        {
            Debug.Log($"[TopBarManager] Coins: {coins}");
            coinsText.text = coins.ToString();
        }

        public void UpdateStageText(int act, int step)
        {
            stageText.SetText("STAGE " + act + "-" + (step + 1));
        }
        
        public void SetClassSelected(string classSelected)
        {
            currentClass = classSelected;
        }

        public void SetHealth(int health)
        {
            currentHealth = health;
        }

        public void ToogleMapIcon(bool arg0)
        {
            showMapButton.SetActive(arg0);
        }
    }
}