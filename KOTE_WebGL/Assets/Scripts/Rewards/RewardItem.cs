using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class RewardItem : MonoBehaviour //, IPointerClickHandler
{
    public TMP_Text rewardText;
    [ReadOnly] public RewardItemType rewardItemType;
    public Image rewardImage;
    public TooltipAtCursor tooltipController;
    private RewardItemData rewardData;

    // the effect gets passed in from the panel so it persists
    private Action<RewardItemType> rewardEffectsAction;

    //and then we pass in the action we want to take when the reward is selected
    private Action onRewardSelected;

    public void PopulateRewardItem(RewardItemData reward, Action<RewardItemType> EffectsAction, Action onAddACard = null)
    {
        rewardEffectsAction = EffectsAction;
        if (onAddACard == null || reward.type != "card")
        {
            onRewardSelected = onRewardClicked;
        }
        else
        {
            onRewardSelected = onAddACard;
        }

        rewardItemType = Utils.ParseEnum<RewardItemType>(reward.type);
        rewardData = reward;
        switch (rewardItemType)
        {
            case RewardItemType.card:
                tooltipController.enabled = false;
                rewardText.text = "Add a card to your deck";
                rewardImage.sprite = SpriteAssetManager.Instance.GetMiscImage("card");
                break;

            case RewardItemType.gold:
                tooltipController.enabled = false;
                rewardText.text = reward.amount + " gold";
                rewardImage.sprite = SpriteAssetManager.Instance.GetMiscImage("coin");
                break;

            case RewardItemType.potion:
                RewardPotion potion = reward.potion;
                rewardText.text = potion.name;
                rewardImage.sprite = SpriteAssetManager.Instance.GetPotionImage(potion.potionId);

                // setup description tooltip
                List<Tooltip> tooltips = new List<Tooltip>
                    { new Tooltip { description = rewardData.potion.description, title = rewardData.potion.name } };
                tooltipController.SetTooltips(tooltips);
                break;

            case RewardItemType.trinket:
                break;

            case RewardItemType.fief:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void onRewardClicked()
    {
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        GameManager.Instance.EVENT_REWARD_SELECTED.Invoke(rewardData.id);
    }

    public void OnPointerClick(PointerEventData data)
    {
        Debug.Log("onClickFired");
        rewardEffectsAction.Invoke(rewardItemType);
        onRewardSelected.Invoke();
    }

    public void OnRewardClaimed()
    {
        Debug.Log("onClickFired");
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
        rewardEffectsAction.Invoke(rewardItemType);
        onRewardSelected.Invoke();
    }
}