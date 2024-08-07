﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Refactor.TopBar
{
    public class PotionsContainer : MonoBehaviour
    {
        public List<PotionManager> potions;
        public GameObject potionPrefab;
        public GameObject potionLayout;
        public Image warningBackground;

        public GameObject potionOptionPanel;
        public Button drinkButton;
        public Button discardButton;
        public TextMeshProUGUI warningText;

        private int potionMax = 3;

        // get notified of the current game status so the potions know if the player is in combat
        private GameStatuses currentGameStatus;
        private PotionManager currentPotion;

        
        public delegate void DrinkPotionEvent(string potionId, string targetId);
        public event DrinkPotionEvent OnDrinkPotion;

        public event Action<string> OnDiscardPotion; 

        public void Init()
        {
            potionOptionPanel.SetActive(false);
            //GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStateUpdate);
            GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.AddListener(OnShowPotionOptions);
            GameManager.Instance.EVENT_POTION_WARNING.AddListener(OnPotionWarning);
            GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameStatusChange);
            
            drinkButton.onClick.AddListener(DrinkPotion);
            discardButton.onClick.AddListener(DiscardPotion);
        }

        private void DrinkPotion()
        {
            Debug.Log($"Drink Potion!");
            if (!currentPotion)
            {
                Debug.LogWarning($"[DrinkPotion] No current potion selected");
                return;
            }
            
            if (currentPotion.ShowsPointer() == true)
            {
                // this turns on the pointer from the potion
                currentPotion.pointerActive = true;
                potionOptionPanel.SetActive(false);
                return;
            }

            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Use Potion");
            OnDrinkPotion?.Invoke(currentPotion.GetPotionId(), null);
            potionOptionPanel.SetActive(false);
            
            currentPotion = null;
        }
        
        private void DiscardPotion()
        {
            Debug.Log($"Discard Potion!");
            if (!currentPotion)
            {
                Debug.LogWarning($"[DiscardPotion] No current potion selected");
                return;
            }
            
            OnDiscardPotion?.Invoke(currentPotion.GetPotionId());
            GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
            potionOptionPanel.SetActive(false);
            
            currentPotion = null;
        }

        public void UpdateState(List<PotionData> potions)
        {
            ClearPotions();
            CreateHeldPotions(potions);
            CreateEmptyPotions();
        }

        private void ClearPotions()
        {
            foreach (PotionManager potion in potions)
            {
                Destroy(potion.gameObject);
            }

            potions.Clear();
        }

        private void CreateHeldPotions(List<PotionData> heldPotions)
        {
            foreach (PotionData potion in heldPotions)
            {
                PotionManager potionManager =
                    Instantiate(potionPrefab, potionLayout.transform).GetComponent<PotionManager>();
                potionManager.Populate(potion);
                potions.Add(potionManager);
            }
        }

        public void CreateEmptyPotions()
        {
            for (int i = potions.Count; i < potionMax; i++)
            {
                PotionManager potion = Instantiate(potionPrefab, potionLayout.transform).GetComponent<PotionManager>();
                potions.Add(potion);
                potion.Populate(null);
            }
        }

        private void OnShowPotionOptions(PotionManager potion)
        {
            potionOptionPanel.SetActive(true);
            if (potion.IsUsableOutsideCombat() == false && currentGameStatus != GameStatuses.Combat)
            {
                drinkButton.interactable = false;
            }
            else
            {
                drinkButton.interactable = true;
            }

            currentPotion = potion;
        }

        private void OnPotionWarning(string action)
        {
            // show the warning in the option box
            drinkButton.gameObject.SetActive(false);
            discardButton.gameObject.SetActive(false);
            warningText.gameObject.SetActive(true);
            switch (action)
            {
                case "potion_not_found_in_database":
                    warningText.text = "Potion Does Not Exist In database";
                    break;
                case "potion_not_in_inventory":
                    warningText.text = "Potion No Longer In Inventory";
                    break;
                case "potion_max_count_reached":
                    warningText.text = "No Space For Another Potion";
                    break;
                case "potion_not_usable_outside_combat":
                    warningText.text = "This Potion Cannot Be Used Outside of Combat";
                    break;
            }

            potionOptionPanel.SetActive(true);


            Sequence sequence = DOTween.Sequence();
            sequence.Join(warningBackground.DOFade(0.5f, 1).SetLoops(4, LoopType.Yoyo)).OnComplete(() =>
            {
                potionOptionPanel.SetActive(false);
                drinkButton.gameObject.SetActive(true);
                discardButton.gameObject.SetActive(true);
                warningText.gameObject.SetActive(false);
            });
            sequence.Play();
        }

        private void OnGameStatusChange(GameStatuses newStatus)
        {
            currentGameStatus = newStatus;
        }
    }
}