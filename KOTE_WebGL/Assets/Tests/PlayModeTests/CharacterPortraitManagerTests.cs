using System.Collections;
using System.Collections.Generic;
using KOTE.UI.Armory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class CharacterPortraitManagerTests : MonoBehaviour
{
    private GameObject characterPortrait;
    private CharacterPortraitManager _portraitManager;

    private Nft testVillager = new Nft
    {
        CanPlay = true,
        Traits = new Dictionary<Trait, string>
        {
            { Trait.Helmet , "Basic Bucket Helmet"},
            { Trait.Padding , "Red"},
            { Trait.Shield , "Rusty Shield"},
            { Trait.Weapon , "Rusty Sword"}
        }
    };
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameObject portraitPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Armory/ArmoryCharacterPortrait.prefab");
        characterPortrait = Instantiate(portraitPrefab);
        _portraitManager = characterPortrait.GetComponent<CharacterPortraitManager>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(characterPortrait);
        PortraitSpriteManager.Instance.DestroyInstance();
        yield return null;
    }

    [Test]
    public void DoesInactiveOverlayExist()
    {
        Assert.NotNull(_portraitManager.inactiveOverlay);
    }

    [Test]
    public void DoesKnightObjectExist()
    {
        Assert.NotNull(_portraitManager.knight);
    }

    [Test]
    public void DoesKnightImageExist()
    {
        Assert.NotNull(_portraitManager.knightImage);
    }

    [Test]
    public void DoesDefaultKnightExist()
    {
        Assert.NotNull(_portraitManager.defaultKnight);
    }

    [Test]
    public void DoesVillagerObjectExist()
    {
        Assert.NotNull(_portraitManager.villager);
    }

    [Test]
    public void DoesVillagerCompositeExist()
    {
        Assert.NotNull(_portraitManager.villagerComposite);
    }
    
    [Test]
    public void DoesLoadingTextExist()
    {
        Assert.NotNull(_portraitManager.loadingText);
    }

    [Test]
    public void DoesPortraitLayersArrayExist()
    {
        Assert.NotNull(_portraitManager.portraitLayers);
    }

    [Test]
    public void DoPortraitLayersExist()
    {
        foreach (Image layer in _portraitManager.portraitLayers)
        {
            Assert.NotNull(layer);
        }
    }

    [Test]
    public void DoesPortraitLayersHaveCorrectNumberOfLayers()
    {
        Assert.AreEqual(4, _portraitManager.portraitLayers.Length);
    }

    [Test]
    public void DoesShowingDefaultHideVillager()
    {
        _portraitManager.SetDefault();
        Assert.False(_portraitManager.villager.activeSelf);
    }
    
    [Test]
    public void DoesShowingDefaultShowKnight()
    {
        _portraitManager.SetDefault();
        Assert.True(_portraitManager.knight.activeSelf);
    }

    [Test]
    public void DoesShowingDefaultShowDefaultImage()
    {
        _portraitManager.SetDefault();
        Assert.AreEqual(_portraitManager.defaultKnight, _portraitManager.knightImage.sprite);
    }

    [Test]
    public void DoesShowingActiveVillagerDisableOverlay()
    {
        _portraitManager.SetPortrait(testVillager);
        Assert.False(_portraitManager.inactiveOverlay.activeSelf);
    }
    
    [Test]
    public void DoesShowingInactiveVillagerEnableOverlay()
    {
        testVillager.CanPlay = false;
        _portraitManager.SetPortrait(testVillager);
        Assert.True(_portraitManager.inactiveOverlay.activeSelf);
        testVillager.CanPlay = true;
    }

    [Test]
    public void DoesCallingSetPortraitFromTestHideCorrectLayers()
    {
        testVillager.Traits[Trait.Helmet] = "None";
        _portraitManager.SetPortrait(testVillager);
        Assert.True(_portraitManager.portraitLayers[0].gameObject.activeSelf);
        Assert.True(_portraitManager.portraitLayers[1].gameObject.activeSelf);
        Assert.True(_portraitManager.portraitLayers[2].gameObject.activeSelf);
        Assert.False(_portraitManager.portraitLayers[3].gameObject.activeSelf);
        testVillager.Traits[Trait.Helmet] = "Basic Bucket Helmet";

    }
}
