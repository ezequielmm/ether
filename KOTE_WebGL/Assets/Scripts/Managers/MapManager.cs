using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public void OnRoyalHousesButton()
    {
        GameManager.Instance.EVENT_ROYALHOUSES_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnShopButton()
    {
        GameManager.Instance.EVENT_SHOPLOCATION_ACTIVATION_REQUEST.Invoke(true);
    }

    public void LoadCombat()
    {
        GameManager.Instance.LoadScene(inGameScenes.Combat);
    }

    public void ShowShop()
    {
    }
}