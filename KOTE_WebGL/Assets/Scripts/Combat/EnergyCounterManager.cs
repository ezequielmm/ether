using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnergyCounterManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI energyTF;
    private RectTransform rectTransform;

    private void Awake()
    {
        //GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
    }
    private void OnEnable()
    {
        GameManager.Instance.EVENT_UPDATE_ENERGY.AddListener(OnEnergyUpdate);
       
        GameManager.Instance.EVENT_GENERIC_WS_DATA.Invoke(WS_DATA_REQUEST_TYPES.Energy);
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnergyUpdate(int arg0, int arg1)
    {
        energyTF.SetText(arg0.ToString() + "/" + arg1.ToString());
    }

    private void Start()
    {
       // GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeStateDateUpdate);
        
    }

    private void OnNodeStateDateUpdate(NodeStateData nodeState, WS_QUERY_TYPE quertyType)
    {
       // if(nodeState.data != null && nodeState.data.data != null)energyTF.SetText(nodeState.data.data.player.energy + "/" + nodeState.data.data.player.energy_max);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 anchorPoint = new Vector3(transform.position.x + (rectTransform.rect.width * rectTransform.lossyScale.x),
            transform.position.y + ((rectTransform.rect.height / 2 * rectTransform.lossyScale.y)), 0);
        anchorPoint = Camera.main.ScreenToWorldPoint(anchorPoint);
        // Tooltip On
        GameManager.Instance.EVENT_SET_TOOLTIPS.Invoke(ToolTipValues.Instance.EnergyCounterTooltips, TooltipController.Anchor.MiddleLeft, anchorPoint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip Off
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.Invoke();
    }
}