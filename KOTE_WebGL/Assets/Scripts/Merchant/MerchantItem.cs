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
    [SerializeField]
    private GameObject soldContainer;

    [SerializeField]
    private Sprite selectedBgSprite;
    [SerializeField]
    private Sprite unSelectedBgSprite;
    [SerializeField]
    private Image backgroundPanel;

    private Button button;

    protected void Awake()
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(OnSelect);
    }

    public UnityEvent<int, MerchantData.IMerchant> PrepareBuy = new UnityEvent<int, MerchantData.IMerchant>();

    protected void SetPrice()
    {
        priceText.text = $"{data.Coin}";
    }

    private void OnSelect()
    {
        PrepareBuy.Invoke(data.Coin, data);
        backgroundPanel.sprite = selectedBgSprite;
    }

    public void OnDeselect()
    {
        // Remove selection data
        backgroundPanel.sprite = unSelectedBgSprite;
    }

    public void CheckAffordability(int coinsInPocket) 
    {
        // if sold
        if (data.IsSold) { return; }

        if (data.Coin <= coinsInPocket)
        {
            // can afford button
            button.interactable = true;
            priceText.color = Color.white;
        } 
        else 
        {
            button.interactable = false;
            priceText.color = Color.red;
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
        priceText.color = Color.white;
        if (!data.IsSold)
        {
            // Is not sold
            soldContainer.SetActive(false);
            button.enabled = true;
        }
        else 
        {
            // Is Sold
            soldContainer.SetActive(true);
            button.enabled = false;
            priceText.text = "SOLD";
            priceText.color = Color.red;
        }
        backgroundPanel.sprite = unSelectedBgSprite;
    }
}
