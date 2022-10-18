using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionsContainerManager : MonoBehaviour
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

    private void Start()
    {
        potionOptionPanel.SetActive(false);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.AddListener(OnPlayerStateUpdate);
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.AddListener(OnShowPotionOptions);
        GameManager.Instance.EVENT_POTION_WARNING.AddListener(OnPotionWarning);
        GameManager.Instance.EVENT_GAME_STATUS_CHANGE.AddListener(OnGameStatusChange);
    }

    private void OnPlayerStateUpdate(PlayerStateData playerState)
    {
        ClearPotions();
        CreateHeldPotions(playerState.data.playerState.potions);
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

    private void CreateHeldPotions(List<Potion> heldPotions)
    {
        foreach (Potion potion in heldPotions)
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

        drinkButton.onClick.AddListener(() =>
        {
            if (potion.ShowsPointer() == true)
            {
                // this turns on the pointer from the potion
                potion.pointerActive = true;
                potionOptionPanel.SetActive(false);
                return;
            }

            GameManager.Instance.EVENT_POTION_USED.Invoke(potion.GetPotionId(), null);
            potionOptionPanel.SetActive(false);
        });
        discardButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EVENT_POTION_DISCARDED.Invoke(potion.GetPotionId());
            potionOptionPanel.SetActive(false);
        });
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
                warningText.text = "Maximum potions in inventory. Use or discard a potion to make room.";
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