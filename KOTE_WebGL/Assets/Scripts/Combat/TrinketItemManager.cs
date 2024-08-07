using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrinketItemManager : MonoBehaviour, IPointerClickHandler
{
    public Image trinketImage;
    public GameObject counter;
    public TMP_Text counterText;
    public TooltipAtCursor tooltipController;
    public Image selectFrame;

    public string Id => _trinket.id;
    private Trinket _trinket;
    private Tooltip _tooltip;
    
    // select variables
    private bool enablePointerClick;
    private Action<bool> onTrinketSelected;
    public bool isToggled;

    private void Start()
    {
        selectFrame.gameObject.SetActive(false);
        GameManager.Instance.EVENT_TRINKET_ACTIVATED.AddListener(OnTrinketTriggered);
    }

    // uses string values for right now, will probably need to change once we parse trinket data
    // selectMode and onSelected are used to allow us to select trinkets from the trinket panel
    public void Populate(Trinket trinket, bool selectMode = false, Action<bool> onSelected = null)
    {
        _trinket = trinket;
        trinketImage.sprite = SpriteAssetManager.Instance.GetTrinketImage(trinket.trinketId);
        _tooltip = new Tooltip()
        {
            title = _trinket.name,
            description = _trinket.description
        };
        tooltipController.SetTooltips(new List<Tooltip> { _tooltip });
        if (selectMode)
        {
            enablePointerClick = true;
            onTrinketSelected = onSelected;
        }

        counter.SetActive(trinket.counter > 0);
        counterText.text = trinket.counter.ToString();
    }

    // utility function to update the toggle status from within the OnTrinketSelected action
    public void UpdateToggleStatus()
    {
        selectFrame.gameObject.SetActive(isToggled);
    }

    private void OnTrinketTriggered(Trinket triggeredTrinket)
    {
        if (_trinket.trinketId == triggeredTrinket.trinketId && _trinket.id == triggeredTrinket.id)
        {
            float oldScale = trinketImage.transform.localScale.x;
            trinketImage.transform.DOScale(2f, 1).OnComplete(() =>
            {
                trinketImage.transform.DOScale(oldScale, 1);
            });
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (enablePointerClick)
        {
            onTrinketSelected?.Invoke(!isToggled);
        }
    }
}