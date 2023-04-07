using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ContestRulesPanelTests : MonoBehaviour
{
    ContestRulesPanel rulesPanel;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject Prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MainMenu/Contest Rules.prefab");
        GameObject Obj = GameObject.Instantiate(Prefab);
        rulesPanel = Obj.GetComponent<ContestRulesPanel>();
        yield return null;
    }

    [TearDown]
    public void MainMenuPrefabExists()
    {
        Destroy(rulesPanel);
    }

    [UnityTest]
    public IEnumerator RulesGetPopulated() 
    {
        string rules = $"a\nb\nc\nd";
        rulesPanel.Populate(rules);
        yield return new WaitForEndOfFrame();
        Assert.AreEqual(4, rulesPanel.RulesContainer.transform.childCount);
    }
    [Test]
    public void PanelEnabledOnToggle() 
    {
        rulesPanel.TogglePannel(true);
        Assert.IsTrue(rulesPanel.Panel.activeSelf);
    }
    [Test]
    public void PanelEnabledOnEnable()
    {
        rulesPanel.EnablePanel();
        Assert.IsTrue(rulesPanel.Panel.activeSelf);
    }
    [Test]
    public void PanelDisabledOnToggle()
    {
        rulesPanel.TogglePannel(false);
        Assert.IsFalse(rulesPanel.Panel.activeSelf);
    }
    [Test]
    public void PanelDisabledOnDisable()
    {
        rulesPanel.DisablePanel();
        Assert.IsFalse(rulesPanel.Panel.activeSelf);
    }
    [Test]
    public void PanelDisabledOnContinue()
    {
        rulesPanel.OnContinueButton();
        Assert.IsFalse(rulesPanel.Panel.activeSelf);
    }
}
