using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class EnemyIntentManagerTests : MonoBehaviour
{
    private EnemyIntentManager _intentManager;
    private GameObject go;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return null;
        go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Enemies/EnemyBase.prefab");
        go = Instantiate(go);
        EnemyManager enemyManager = go.GetComponent<EnemyManager>();
        enemyManager.EnemyData = new EnemyData
        {
            category = "basic",
            defense = 0,
            enemyId = 1,
            hpCurrent = 42,
            hpMax = 42,
            name = "Sporemonger",
            id = "TestEnemy",
            size = "small",
            type = "plant"
        };
        _intentManager = go.GetComponentInChildren<EnemyIntentManager>();
        go.SetActive(true);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
        Destroy(_intentManager.gameObject);
        Destroy(go);
        Destroy(GameManager.Instance.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DoesStartLogMissingIconLogErrorMessage()
    {
        GameObject intentManagerGameObject = new GameObject();
        GameObject enemyIntentManager = Instantiate(intentManagerGameObject);
        enemyIntentManager.transform.SetParent(go.transform);
        _intentManager = enemyIntentManager.AddComponent<EnemyIntentManager>();
        SerializedObject so = new SerializedObject(_intentManager);
        so.FindProperty("iconContainer").objectReferenceValue = enemyIntentManager.AddComponent<SpriteSpacer>();
        so.ApplyModifiedProperties();
        enemyIntentManager.SetActive(true);
        yield return null;
        LogAssert.Expect(LogType.Error, "[EnemyIntentManager] Missing Icon Prefab.");
    }

    [UnityTest]
    public IEnumerator DoesStartLogMissingIconContainerLogErrorMessage()
    {
        GameObject intentManagerGameObject = new GameObject();
        GameObject enemyIntentManager = Instantiate(intentManagerGameObject);
        enemyIntentManager.transform.SetParent(go.transform);
        _intentManager = enemyIntentManager.AddComponent<EnemyIntentManager>();
        SerializedObject so = new SerializedObject(_intentManager);
        so.FindProperty("iconPrefab").objectReferenceValue = new GameObject();
        so.ApplyModifiedProperties();
        enemyIntentManager.SetActive(true);
        yield return null;
        LogAssert.Expect(LogType.Error, "[EnemyIntentManager] Missing Icon Container.");
        LogAssert.Expect(LogType.Exception,
            "NullReferenceException: Object reference not set to an instance of an object");
    }

    [UnityTest]
    public IEnumerator DoesStartLogMissingParentEnemyLogErrorMessage()
    {
        GameObject intentManagerGameObject = new GameObject();
        GameObject enemyIntentManager = Instantiate(intentManagerGameObject);
        _intentManager = enemyIntentManager.AddComponent<EnemyIntentManager>();
        SerializedObject so = new SerializedObject(_intentManager);
        so.FindProperty("iconContainer").objectReferenceValue = enemyIntentManager.AddComponent<SpriteSpacer>();
        so.FindProperty("iconPrefab").objectReferenceValue = new GameObject();
        so.ApplyModifiedProperties();
        enemyIntentManager.SetActive(true);
        yield return null;
        LogAssert.Expect(LogType.Error, "[EnemyIntentManager] Manager does not belong to an enemy.");
    }

    [UnityTest]
    public IEnumerator DoesOnMouseEnterSetTooltips()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener(((arg0, anchor, vector3, arg3) => { eventFired = true; }));
        _intentManager.CallUnityPrivateMethod("OnMouseEnter");
        yield return null;
        Assert.True(eventFired);
    }

    [Test]
    public void DoesOnMouseEnterSendCorrectTooltips()
    {
        bool eventFired = false;
        bool correctTooltipName = false;
        bool correctTooltipDescription = false;
        GameManager.Instance.EVENT_SET_TOOLTIPS.AddListener(((tooltips, anchor, vector3, arg3) =>
        {
            eventFired = true;
            if (tooltips[0].title == "Attack") correctTooltipName = true;
            if (tooltips[0].description == "intent") correctTooltipDescription = true;
        }));

        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        _intentManager.CallUnityPrivateMethod("OnMouseEnter");
        Assert.True(eventFired);
        Assert.True(correctTooltipName);
        Assert.True(correctTooltipDescription);
    }

    [Test]
    public void DoesOnMouseExitClearTooltips()
    {
        bool eventFired = false;
        GameManager.Instance.EVENT_CLEAR_TOOLTIPS.AddListener((() => { eventFired = true; }));
        _intentManager.CallUnityPrivateMethod("OnMouseExit");
        Assert.True(eventFired);
    }

    [UnityTest]
    public IEnumerator DoesUpdateAskForIntent()
    {
        bool eventFired = false;
        bool correctRequestType = false;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) =>
        {
            eventFired = true;
            if (data == WS_DATA_REQUEST_TYPES.EnemyIntents) correctRequestType = true;
        });
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke("enemy");
        yield return null;
        Assert.True(eventFired);
        Assert.True(correctRequestType);
    }

    [Test]
    public void DoesOnTurnChangeAskForIntent()
    {
        bool eventFired = false;
        bool correctRequestType = false;
        GameManager.Instance.EVENT_GENERIC_WS_DATA.AddListener((data) =>
        {
            eventFired = true;
            if (data == WS_DATA_REQUEST_TYPES.EnemyIntents) correctRequestType = true;
        });
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke("enemy");
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent());
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke("player");
        Assert.True(eventFired);
        Assert.True(correctRequestType);
    }

    [UnityTest]
    public IEnumerator DoesOnTurnChangeClearIntentIcons()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual(1, intentIcons.Length);
        GameManager.Instance.EVENT_CHANGE_TURN.Invoke("enemy");
        yield return new WaitForSeconds(0.3f);
        intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual(0, intentIcons.Length);
    }

    [UnityTest]
    public IEnumerator DoesUpdatingIntentsClearIntentIcons()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual(1, intentIcons.Length);
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
            { id = "TestEnemy", intents = new List<EnemyIntent.Intent>() });
        yield return new WaitForSeconds(0.3f);
        intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual(0, intentIcons.Length);
    }

    [Test]
    public void DoesUpdatingIntentsCreateIntentIcon()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual(1, intentIcons.Length);
    }

    [Test]
    public void DoesUpdatingIntentsCreateIntentIconWithCorrectImage()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("ATK_icon01", intentIcons[0].sprite.name);
    }

    [Test]
    public void DoesSendingAttackIntentsSetCorrectAttackImage()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
            {
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 6 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 11 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 16 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 21 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 26 },
                new EnemyIntent.Intent { description = "intent", type = "attack", value = 31 }
            }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("ATK_icon01", intentIcons[0].sprite.name);
        Assert.AreEqual("ATK_icon02", intentIcons[1].sprite.name);
        Assert.AreEqual("ATK_icon03", intentIcons[2].sprite.name);
        Assert.AreEqual("ATK_icon04", intentIcons[3].sprite.name);
        Assert.AreEqual("ATK_icon05", intentIcons[4].sprite.name);
        Assert.AreEqual("ATK_icon06", intentIcons[5].sprite.name);
        Assert.AreEqual("ATK_icon07", intentIcons[6].sprite.name);
    }

    
    [Test]
    public void DoesSendingAttackingIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attacking", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("ATK_icon01", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingDefendIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "defend", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("DEF", intentIcons[0].sprite.name);
    }

    [Test]
    public void DoesSendingDefendingIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "defending", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("DEF", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingPlotIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "plot", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("plotting", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingBuffIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "buff", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("plotting", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingPlottingIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "plotting", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("plotting", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingSchemeIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "scheme", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Scheming", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingSchemeingIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "schemeing", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Scheming", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingDebuffIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "debuff", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Scheming", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingStunIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "stun", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Stunned", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingNothingIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "nothing", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Stunned", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingStunnedIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "stunned", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Stunned", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingUnknownIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "unknown", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Unknow", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesSendingUnrecognizedIntentSelectCorrectIntent()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "test", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        Assert.AreEqual("Unknow", intentIcons[0].sprite.name);
    }
    
    [Test]
    public void DoesUpdatingIntentsCreateIntentIconWithCorrectDamageValue()
    {
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Image[] intentIcons = GameObject.Find("Sprite Container").GetComponentsInChildren<Image>();
        TMP_Text valueText = intentIcons[0].GetComponentInChildren<TMP_Text>();
        Assert.AreEqual("1", valueText.text);
    }

    [Test]
    public void DoesUpdatingIntentsActivateIntentsCollider()
    {
        BoxCollider2D intentsCollider = _intentManager.gameObject.GetComponent<BoxCollider2D>();
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Assert.True(intentsCollider.enabled);
    }

    [Test]
    public void DoesActivatingPointerDeactivateIntentsCollider()
    {
        BoxCollider2D intentsCollider = _intentManager.gameObject.GetComponent<BoxCollider2D>();
        GameManager.Instance.EVENT_UPDATE_INTENT.Invoke(new EnemyIntent
        {
            id = "TestEnemy",
            intents = new List<EnemyIntent.Intent>
                { new EnemyIntent.Intent { description = "intent", type = "attack", value = 1 } }
        });
        Assert.True(intentsCollider.enabled);
        GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(new PointerData());
        Assert.False(intentsCollider.enabled);
    }
    
    [Test]
    public void DoesDeactivatingPointerActivateIntentsCollider()
    {
        BoxCollider2D intentsCollider = _intentManager.gameObject.GetComponent<BoxCollider2D>();
        GameManager.Instance.EVENT_ACTIVATE_POINTER.Invoke(new PointerData());
        Assert.False(intentsCollider.enabled);
        GameManager.Instance.EVENT_DEACTIVATE_POINTER.Invoke("");
        Assert.True(intentsCollider.enabled);
    }
}