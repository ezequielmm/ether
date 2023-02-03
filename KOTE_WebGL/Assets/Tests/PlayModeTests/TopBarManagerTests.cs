using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TopBarManagerTests : MonoBehaviour
{
    private TopBarManager _topBarManager;
    private GameObject go;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject go = new GameObject();
        Camera camera = go.AddComponent<Camera>();
        camera.tag = "MainCamera";
        GameObject TopBarPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/TopBarPanel.prefab");
        GameObject TopBar = GameObject.Instantiate(TopBarPrefab);
        _topBarManager = TopBar.GetComponent<TopBarManager>();
        TopBar.SetActive(true);

        yield return null;
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(go);
        Destroy(_topBarManager.gameObject);
        yield return null;
    }

    [Test]
    public void IsNameTextSetToCorrectNumberOfLines()
    {
        Assert.AreEqual(2, _topBarManager.nameText.maxVisibleLines);
    }

    [Test]
    public void DoesToggleTopMapIconEventToggleIcon()
    {
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(true);
        Assert.True(_topBarManager.showmapbutton.activeSelf);
        GameManager.Instance.EVENT_TOOGLE_TOPBAR_MAP_ICON.Invoke(false);
        Assert.False(_topBarManager.showmapbutton.activeSelf);
    }

    [Test]
    public void DoesUpdateCurrentStepChangeStepTest()
    {
        Assert.AreEqual("STAGE ?-?", _topBarManager.stageText.text);
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.Invoke(1, 2);
        Assert.AreEqual("STAGE " + 1 + "-" + 3, _topBarManager.stageText.text);
        GameManager.Instance.EVENT_UPDATE_CURRENT_STEP_INFORMATION.Invoke(-1, 56342);
        Assert.AreEqual("STAGE " + -1 + "-" + 56343, _topBarManager.stageText.text);
    }

    [Test]
    public void DoesSetNameTextUpdateNameText()
    {
        _topBarManager.SetNameText("test");
        Assert.AreEqual("test", _topBarManager.nameText.text);
        _topBarManager.SetNameText("Bored");
        Assert.AreEqual("Bored", _topBarManager.nameText.text);
    }

    [Test]
    public void DoesSetHealthTextUpdateHealthText()
    {
        _topBarManager.SetHealthText(0);
        Assert.AreEqual("0/0", _topBarManager.healthBarText.text);
        _topBarManager.SetHealthText(55);
        Assert.AreEqual("55/0", _topBarManager.healthBarText.text);
        _topBarManager.SetHealthText(-69);
        Assert.AreEqual("-69/0", _topBarManager.healthBarText.text);
    }

    [Test]
    public void DoesSetCoinsTextUpdateCoinsText()
    {
        _topBarManager.SetCoinsText(0);
        Assert.AreEqual("0", _topBarManager.coinsText.text);
        _topBarManager.SetCoinsText(420);
        Assert.AreEqual("420", _topBarManager.coinsText.text);
        _topBarManager.SetCoinsText(-8008);
        Assert.AreEqual("-8008", _topBarManager.coinsText.text);
    }
    

    [Test]
    public void DoesOnPlayerAttackUpdatePlayerHealthIfPlayerIsTheTarget()
    {
        _topBarManager.OnPlayerAttacked(new CombatTurnData
        {
            attackId = Guid.NewGuid(),
            delay = 1,
            originId = "player",
            originType = "test",
            targets = new List<CombatTurnData.Target>
            {
                new CombatTurnData.Target
                {
                    targetId = "player",
                    targetType = "player",
                    healthDelta = 3,
                    finalHealth = 12
                }
            }
        });
        Assert.AreEqual("12/0", _topBarManager.healthBarText.text);
    }

    [Test]
    public void DoesOnPlayerAttackNotUpdatePlayerHealthIfPlayerIsNotTheTarget()
    {
        _topBarManager.SetHealthText(1);
        _topBarManager.OnPlayerAttacked(new CombatTurnData
        {
            attackId = Guid.NewGuid(),
            delay = 1,
            originId = "player",
            originType = "test",
            targets = new List<CombatTurnData.Target>
            {
                new CombatTurnData.Target
                {
                    targetId = "steve",
                    targetType = "steve",
                    healthDelta = 3,
                    finalHealth = 12
                }
            }
        });
        Assert.AreEqual("1/0", _topBarManager.healthBarText.text);
    }

    [Test]
    public void IsOnPlayerAttackTriggeredByAttackResponseEvent()
    {
        GameManager.Instance.EVENT_ATTACK_RESPONSE.Invoke(new CombatTurnData
        {
            attackId = Guid.NewGuid(),
            delay = 1,
            originId = "player",
            originType = "test",
            targets = new List<CombatTurnData.Target>
            {
                new CombatTurnData.Target
                {
                    targetId = "player",
                    targetType = "player",
                    healthDelta = 3,
                    finalHealth = 69
                }
            }
        });
        Assert.AreEqual("69/0", _topBarManager.healthBarText.text);
    }

    [Test]
    public void DoesPlayerStateUpdateUpdateNameText()
    {
        _topBarManager.OnPlayerStatusUpdate(new PlayerStateData
        {
            data = new PlayerStateData.Data
            {
                playerState = new PlayerData
                {
                    playerName = "test",
                    hpMax = 100,
                    hpCurrent = 89,
                    gold = 4556
                }
            }
        });
        Assert.AreEqual("test", _topBarManager.nameText.text);
    }

    [Test]
    public void DoesPlayerStateUpdateUpdateCurrentAndMaxHealth()
    {
        _topBarManager.OnPlayerStatusUpdate(new PlayerStateData
        {
            data = new PlayerStateData.Data
            {
                playerState = new PlayerData
                {
                    playerName = "test",
                    hpMax = 100,
                    hpCurrent = 89,
                    gold = 4556
                }
            }
        });
        Assert.AreEqual("89/100", _topBarManager.healthBarText.text);
    }

    [Test]
    public void DoesPlayerStateUpdateUpdateCurrentGold()
    {
        _topBarManager.OnPlayerStatusUpdate(new PlayerStateData
        {
            data = new PlayerStateData.Data
            {
                playerState = new PlayerData
                {
                    playerName = "test",
                    hpMax = 100,
                    hpCurrent = 89,
                    gold = 4556
                }
            }
        });
        Assert.AreEqual("4556", _topBarManager.coinsText.text);
    }

    [Test]
    public void DoesPlayerStateUpdateEventTriggerPlayerStateUpdate()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(new PlayerStateData
        {
            data = new PlayerStateData.Data
            {
                playerState = new PlayerData
                {
                    playerName = "Boyo",
                    hpMax = 80,
                    hpCurrent = 12,
                    gold = 75,
                    potions = new List<PotionData>()
                }
            }
        });
        Assert.AreEqual("Boyo", _topBarManager.nameText.text);
        Assert.AreEqual("12/80", _topBarManager.healthBarText.text);
        Assert.AreEqual("75", _topBarManager.coinsText.text);
    }

    [Test]
    public void DoesSetClassSelectedUpdateCurrentClass()
    {
        _topBarManager.SetClassSelected("knight");
        Assert.AreEqual("knight", _topBarManager.currentClass);
        _topBarManager.SetClassSelected("rogue");
        Assert.AreEqual("rogue", _topBarManager.currentClass);
    }

    [Test]
    public void DoesClickingMapButtonFireMapIconClickedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(() => { eventFired = true; });
        _topBarManager.OnMapButtonClicked();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingSettingsButtonFireSettingsPanelActivationRequest()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SETTINGSPANEL_ACTIVATION_REQUEST.AddListener((data) => { eventFired = true; });
        _topBarManager.OnSettingsButton();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingDeckButtonFireCardPileClickedEvent()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CARD_PILE_CLICKED.AddListener((data) => { eventFired = true; });
        _topBarManager.OnDeckButtonClicked();
        Assert.True(eventFired);
    }
}