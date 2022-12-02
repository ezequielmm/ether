using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureManager : MonoBehaviour
{
    [SerializeField] private GameObject treasureContainer;
    [SerializeField] private GameObject treasureSprite;
    [SerializeField] private Button openButton;
    [SerializeField] private Button skipButton;

    void Start()
    {
        treasureContainer.SetActive(false);
        GameManager.Instance.EVENT_TOOGLE_TREASURE_ELEMENTS.AddListener(ContainerToggle);
        GameManager.Instance.EVENT_TREASURE_CHEST_SIZE.AddListener(OnGetChestSize);
        GameManager.Instance.EVENT_TREASURE_CHEST_RESULT.AddListener(OnChestOpened);
    }

    public void OpenChest()
    {
        GameManager.Instance.EVENT_TREASURE_OPEN_CHEST.Invoke();
    }

    public void OnTreasureDone()
    {
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }

    private void ContainerToggle(bool value)
    {
        treasureContainer.SetActive(value);
        if (value == true)
        {
            GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.TreasureData);
        }

        openButton.gameObject.SetActive(value);
        skipButton.gameObject.SetActive(value);
    }

    private void OnGetChestSize(SWSM_TreasureData data)
    {
        switch (data.data.data)
        {
            case "small":
                break;
            case "medium":
                treasureSprite.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case "large":
                treasureSprite.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                break;
        }
    }

    private void OnChestOpened(SWSM_ChestResult chestResult)
    {
        openButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
       // Debug.LogError("Trapped Type: " + chestResult.data.data.trapped.trappedType);
        switch (chestResult.data.data.trapped.trappedType)
        {
            case "combat":
                OnCombatTrap(chestResult);
                break;
            case "card":
                OnCardTrap(chestResult);
                break;
            case "damage":
                OnDamageTrap(chestResult);
                break;
            default:
                ShowRewardsPanel();
                break;
        }
    }

    private void OnCombatTrap(SWSM_ChestResult chestResult)
    {
        GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT_WITH_ON_COMPLETE.Invoke(
            chestResult.data.data.trapped.trappedText,
            () => { GameManager.Instance.EVENT_START_COMBAT_ENCOUNTER.Invoke(); });
    }

    private void OnCardTrap(SWSM_ChestResult chestResult)
    {
        ShowTrappedMessage(chestResult);
    }

    private void OnDamageTrap(SWSM_ChestResult chestResult)
    {
        GameManager.Instance.EVENT_ENCOUNTER_DAMAGE.Invoke(chestResult.data.data.trapped.damage);
        ShowTrappedMessage(chestResult);
    }

    private void ShowTrappedMessage(SWSM_ChestResult chestResult)
    {
        GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT_WITH_ON_COMPLETE.Invoke(
            chestResult.data.data.trapped.trappedText,
            ShowRewardsPanel);
    }

    private void ShowRewardsPanel()
    {
        // have to package the rewards in a SWSM_RewardsData to send it
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Rewards);
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
    }
}