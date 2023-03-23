using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class SettingsButtonManagerTests : MonoBehaviour
{
    private GameObject settingsButton;
    private SettingsButtonManager _buttonManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject SettingsButtonPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/SettingsBT.prefab");
        settingsButton = Instantiate(SettingsButtonPrefab);
        _buttonManager = settingsButton.GetComponent<SettingsButtonManager>();
        yield return null;
    }

    [UnityTearDown]
    public void TearDown()
    {
        Destroy(settingsButton);
    }

    [Test]
    public void DoesButtonExist()
    {
        Assert.NotNull(_buttonManager.button);
    }

    [Test]
    public void DoesOnAuthenticateActivateSettingsButton()
    {
        settingsButton.SetActive(false);
        GameManager.Instance.EVENT_AUTHENTICATED.Invoke();
        Assert.True(_buttonManager.button.activeSelf);
    }

    [Test]
    public void DoesOnLogoutSuccessfulDeactivateSettingsButton()
    {
        settingsButton.SetActive(true);
        GameManager.Instance.EVENT_REQUEST_LOGOUT_SUCCESSFUL.Invoke(null);
        Assert.False(_buttonManager.button.activeSelf);
    }
    
    [Test]
    public void DoesOnSettingsActivatedDeactivateSettingsButton()
    {
        settingsButton.SetActive(true);
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(true);
        Assert.False(_buttonManager.button.activeSelf);
    }
    
    [Test]
    public void DoesOnSettingsActivatedActivateSettingsButton()
    {
        settingsButton.SetActive(false);
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.Invoke(false);
        Assert.True(_buttonManager.button.activeSelf);
    }

    [Test]
    public void DoesClickingButtonFireShowSettings()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener((data) => { eventFired = true; });
        _buttonManager.OnSettingsButton();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingButtonFirePlaySfx()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { eventFired = true; });
        _buttonManager.OnSettingsButton();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingButtonFirePlaySfxOfCorrectType()
    {
        bool correctType = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { correctType = data == SoundTypes.UI; });
        _buttonManager.OnSettingsButton();
        Assert.True(correctType);
    }

    [Test]
    public void DoesClickingButtonFirePlaySfxWithCorrectName()
    {
        bool correctName = false;
        GameManager.Instance.EVENT_PLAY_SFX.AddListener((data, data2) => { correctName = data2 == "Button Click"; });
        _buttonManager.OnSettingsButton();
        Assert.True(correctName);
    }
}