using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class PotionsContainerManagerTests : MonoBehaviour
{
    private GameObject cursorObject;
    private GameObject go;
    private GameObject spriteManager;
    private GameObject potionsContainerGameObject;
    private PotionsContainerManager _potionsContainerManager;

    private PlayerStateData emptyPlayerState = new PlayerStateData
    {
        data = new PlayerStateData.Data
        {
            playerState = new PlayerData
            {
                potions = new List<PotionData>()
            }
        }
    };

    private PlayerStateData potionPlayerState = new PlayerStateData
    {
        data = new PlayerStateData.Data
        {
            playerState = new PlayerData
            {
                potions = new List<PotionData>
                {
                    new PotionData
                    {
                        cost = 0,
                        effects = new List<Effect>(),
                        description = "test",
                        id = "test",
                        name = "test",
                        potionId = 1,
                        rarity = "rare",
                        showPointer = false,
                        usableOutsideCombat = true
                    },
                    new PotionData
                    {
                        cost = 0,
                        effects = new List<Effect>(),
                        description = "test",
                        id = "test",
                        name = "test",
                        potionId = 1,
                        rarity = "rare",
                        showPointer = false,
                        usableOutsideCombat = true
                    },
                    new PotionData
                    {
                        cost = 0,
                        effects = new List<Effect>(),
                        description = "test",
                        id = "test",
                        name = "test",
                        potionId = 1,
                        rarity = "rare",
                        showPointer = false,
                        usableOutsideCombat = true
                    }
                }
            }
        }
    };

    private PotionData _potionData = new PotionData
    {
        cost = 0,
        effects = new List<Effect>(),
        description = "test",
        id = "test",
        name = "test",
        potionId = 1,
        rarity = "rare",
        showPointer = false,
        usableOutsideCombat = false
    };

    private PotionManager dummyPotion;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        go = new GameObject();
        Camera camera = go.AddComponent<Camera>();
        camera.tag = "MainCamera";

        cursorObject = new GameObject();
        cursorObject.AddComponent<Cursor>();
        cursorObject.SetActive(true);
        yield return null;

        GameObject spriteManagerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SpriteManager.prefab");
        spriteManager = Instantiate(spriteManagerPrefab);
        spriteManager.SetActive(true);
        yield return null;

        GameObject potionsContainerPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/PotionsContainer.prefab");
        potionsContainerGameObject = Instantiate(potionsContainerPrefab);
        _potionsContainerManager = potionsContainerGameObject.GetComponent<PotionsContainerManager>();
        potionsContainerGameObject.SetActive(true);
        yield return null;

        GameObject potionPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/BattleUI/PotionPrefab.prefab");
        dummyPotion = Instantiate(potionPrefab).GetComponent<PotionManager>();
        dummyPotion.Populate(_potionData);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(cursorObject);
        Destroy(go);
        Destroy(spriteManager);
        Destroy(dummyPotion.gameObject);
        Destroy(potionsContainerGameObject);
        yield return null;
    }

    [Test]
    public void IsPotionOptionPanelDeactivatedOnStart()
    {
        Assert.False(_potionsContainerManager.potionOptionPanel.activeSelf);
    }

    [Test]
    public void DoesPlayerStateUpdateCreateRealPotions()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(potionPlayerState);
        Assert.AreEqual(3, _potionsContainerManager.potions.Count);
        foreach (PotionManager potion in _potionsContainerManager.potions)
        {
            Assert.AreEqual("test", potion.GetPotionId());
        }
    }

    [UnityTest]
    public IEnumerator DoesPlayerStateUpdateClearPotions()
    {
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(potionPlayerState);
        yield return null;
        Assert.AreEqual(3, _potionsContainerManager.potions.Count);
        GameManager.Instance.EVENT_PLAYER_STATUS_UPDATE.Invoke(emptyPlayerState);
        yield return null;
        Assert.AreEqual(3, _potionsContainerManager.potions.Count);
        Assert.AreNotEqual(6, _potionsContainerManager.potions.Count);
    }

    [Test]
    public void DoesShowPotionMenuActivatePotionMenu()
    {
        Assert.False(_potionsContainerManager.potionOptionPanel.activeSelf);
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(dummyPotion);
        Assert.True(_potionsContainerManager.potionOptionPanel.activeSelf);
    }

    [Test]
    public void DoesShowPotionMenuDisableDrinkButtonIfPotionIsOnlyAvailableInCombat()
    {
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(dummyPotion);
        Assert.False(_potionsContainerManager.drinkButton.interactable);
    }

    [Test]
    public void DoesClickingDrinkButtonFirePotionUsedEvent()
    {
        dummyPotion.Populate(new PotionData
        {
            cost = 0,
            effects = new List<Effect>(),
            description = "test",
            id = "test",
            name = "test",
            potionId = 1,
            rarity = "rare",
            showPointer = false,
            usableOutsideCombat = true
        });
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(dummyPotion);
        bool eventFired = false;
        GameManager.Instance.EVENT_POTION_USED.AddListener((data, data2) => { eventFired = true; });
        _potionsContainerManager.drinkButton.onClick.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesClickingDrinkButtonCloseOptionPanel()
    {
        dummyPotion.Populate(new PotionData
        {
            cost = 0,
            effects = new List<Effect>(),
            description = "test",
            id = "test",
            name = "test",
            potionId = 1,
            rarity = "rare",
            showPointer = false,
            usableOutsideCombat = true
        });
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(dummyPotion);
        _potionsContainerManager.drinkButton.onClick.Invoke();
        Assert.False(_potionsContainerManager.potionOptionPanel.activeSelf);
    }

    [Test]
    public void DoesClickingDiscardButtonFirePotionDiscardedEvent()
    {
        dummyPotion.Populate(new PotionData
        {
            cost = 0,
            effects = new List<Effect>(),
            description = "test",
            id = "test",
            name = "test",
            potionId = 1,
            rarity = "rare",
            showPointer = false,
            usableOutsideCombat = true
        });
        GameManager.Instance.EVENT_POTION_SHOW_POTION_MENU.Invoke(dummyPotion);
        bool eventFired = false;
        GameManager.Instance.EVENT_POTION_DISCARDED.AddListener((data) => { eventFired = true; });
        _potionsContainerManager.discardButton.onClick.Invoke();
        Assert.True(eventFired);
    }

    [Test]
    public void DoesPotionWarningHideOptionButtons()
    {
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("");
        Assert.False(_potionsContainerManager.drinkButton.isActiveAndEnabled);
        Assert.False(_potionsContainerManager.discardButton.isActiveAndEnabled);
    }

    [Test]
    public void DoesPotionWarningShowWarningText()
    {
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("");
        Assert.True(_potionsContainerManager.warningText);
    }

    [Test]
    public void DoesPotionWarningShowCorrectWarningText()
    {
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("potion_not_found_in_database");
        Assert.AreEqual("Potion Does Not Exist In database", _potionsContainerManager.warningText.text);
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("potion_not_in_inventory");
        Assert.AreEqual("Potion No Longer In Inventory", _potionsContainerManager.warningText.text);
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("potion_max_count_reached");
        Assert.AreEqual("No Space For Another Potion",
            _potionsContainerManager.warningText.text);
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("potion_not_usable_outside_combat");
        Assert.AreEqual("This Potion Cannot Be Used Outside of Combat", _potionsContainerManager.warningText.text);
    }

    [UnityTest]
    public IEnumerator DoesOptionPanelResetAfterPotionWarning()
    {
        GameManager.Instance.EVENT_POTION_WARNING.Invoke("potion_not_found_in_database");
        yield return new WaitForSeconds(4.01f);
        Assert.False(_potionsContainerManager.potionOptionPanel.activeSelf);
        Assert.True(_potionsContainerManager.drinkButton.gameObject.activeSelf);
        Assert.True(_potionsContainerManager.discardButton.gameObject.activeSelf);
        Assert.False(_potionsContainerManager.warningText.gameObject.activeSelf);
    }
}