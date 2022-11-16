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

    private void ContainerToggle(bool value)
    {
        treasureContainer.SetActive(value);
        if (value == true)
        {
            GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(false);
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
                treasureSprite.transform.localScale = new Vector3(2, 2, 2);
                break;
            case "large":
                treasureSprite.transform.localScale = new Vector3(3, 3, 3);
                break;
        }
    }

    private void OnChestOpened(SWSM_ChestResult chestResult)
    {
        openButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        if (!string.IsNullOrEmpty(chestResult.data.data.trappedText))
        {
            GameManager.Instance.EVENT_SHOW_COMBAT_OVERLAY_TEXT.Invoke(chestResult.data.data.trappedText);
        }
        GameManager.Instance.EVENT_SHOW_REWARDS_PANEL.Invoke(true);
    }

    private void StartCombat()
    {
        // Enable combat UI
        GameManager.Instance.EVENT_TOOGLE_COMBAT_ELEMENTS.Invoke(true);
        // Disable treasure container
        ContainerToggle(false);
    }

    public void OpenChest()
    {
        GameManager.Instance.EVENT_TREASURE_OPEN_CHEST.Invoke();
    }

    public void OnTreasureDone()
    {
        GameManager.Instance.EVENT_CONTINUE_EXPEDITION.Invoke();
    }
}