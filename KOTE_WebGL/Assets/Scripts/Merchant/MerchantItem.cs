using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MerchantItem<T> : MonoBehaviour where T : MerchantData.IMerchant
{
    protected T data;
    [SerializeField]
    private TextMeshProUGUI priceText;
    private GameObject soldContainer;

    private Button button;

    protected void Awake()
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(OnSelect);
    }

    public UnityEvent<int, MerchantData.IMerchant> PrepareBuy = new UnityEvent<int, MerchantData.IMerchant>();

    protected void SetPrice()
    {
        priceText.text = $"{data.coin}";
    }

    private void OnSelect()
    {
        PrepareBuy.Invoke(data.coin, data);
    }

    public void OnDeselect()
    {
        if (data.is_sale)
        {
            // Remove selection data

        }
    }

    public void CheckAffordability(int coinsInPocket) 
    {
        // if not forsale
        if (!data.is_sale) { return; }

        if (data.coin <= coinsInPocket)
        {
            // can afford button
            button.interactable = true;
        } 
        else 
        {
            button.interactable = false; 
        }
    }

    public void Disable() 
    {
        button.interactable = false;
    }

    public virtual void Populate(T data) 
    {
        this.data = data;
        SetPrice();
        if (!data.is_sale)
        {
            soldContainer.SetActive(true);
        }
        else 
        {
            soldContainer.SetActive(false);
        }
    }
}
