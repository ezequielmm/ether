using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopLocationManager : MonoBehaviour
{
    public GameObject screen1Container, screen2Container, shopLocationContainer;

    public void ActivateInnerShopLocationPanel(bool activate)
    {
        shopLocationContainer.SetActive(activate);
    }

    public void ActivateInnerScreen1Panel(bool activate)
    {
        screen1Container.SetActive(activate);
    }

    public void ActivateInnerScreen2Panel(bool activate)
    {
        screen2Container.SetActive(activate);
    }
}