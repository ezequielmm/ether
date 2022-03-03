using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText, moneyText;

    public Button playButton, treasuryButton, registerButton, loginButton;

    private void Start()
    {
        GameManager.Instance.EVENT_LOGIN_COMPLETED.AddListener(ActivateLabels);

        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(false);
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(false);

        ActivateLabels("", "", 0, false);
    }

    public void ActivateLabels(string name, string token, int fief, bool validLoginLocal)
    {
        nameText.text = name;
        moneyText.text = fief.ToString();

        nameText.gameObject.SetActive(validLoginLocal);
        moneyText.gameObject.SetActive(validLoginLocal);

        playButton.interactable = validLoginLocal;
        treasuryButton.interactable = validLoginLocal;

        registerButton.gameObject.SetActive(!validLoginLocal);
        loginButton.gameObject.SetActive(!validLoginLocal);
    }

    public void OnRegisterButton()
    {
        GameManager.Instance.EVENT_REGISTERPANEL_ACTIVATION_REQUEST.Invoke(true);
    }

    public void OnLoginButton()
    {
        GameManager.Instance.EVENT_LOGINPANEL_ACTIVATION_REQUEST.Invoke(true);
    }
}