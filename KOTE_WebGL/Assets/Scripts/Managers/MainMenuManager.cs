using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainMenuManager : MenuButtons
{
    [SerializeField]
    GameObject LoginPanelPrefab, RegisterPanelPrefab;

    public void Login()
    {
        GameObject LoginPanel = Instantiate(LoginPanelPrefab, this.transform.parent);
        LoginPanel.GetComponent<LoginPanelManager>().mainMenu = this.gameObject;
        this.gameObject.SetActive(false);
    }

    public void Register()
    {
        GameObject RegistryPanel = Instantiate(RegisterPanelPrefab, this.transform.parent);
        RegistryPanel.GetComponent<RegistryPanelManager>().mainMenu = this.gameObject;
        this.gameObject.SetActive(false);
    }
}
