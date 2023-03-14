using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class ScoreboardPanelTests : MonoBehaviour
{
    ScoreboardPanelManager scoreboardPanel;
    ScoreboardData testData = new ScoreboardData() 
    {
        Outcome = "outcome",
        ExpeditionType = "expedition",
        TotalScore = 101,
        Achievements= new List<Achievement>() {
        new Achievement()
        {
            Name = "Achievement",
            Score = 281
        }
        },
        NotifyNoLoot = false,
        Lootbox = new()
    };

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        GameObject Prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/ScoreboardPanel.prefab");
        GameObject Object = Instantiate(Prefab);
        scoreboardPanel = Object.GetComponent<ScoreboardPanelManager>();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if(scoreboardPanel != null)
            Destroy(scoreboardPanel.gameObject);
        yield return null;
    }

    [Test]
    public void PanelToggleOn()
    {
        scoreboardPanel.TogglePanel(true);
        Assert.AreEqual(true, scoreboardPanel.gameOverContainer.activeSelf);
    }

    [Test]
    public void PanelToggleOff()
    {
        scoreboardPanel.TogglePanel(false);
        Assert.AreEqual(false, scoreboardPanel.gameOverContainer.activeSelf);
    }

    [Test]
    public void PopulateOutcome()
    {
        scoreboardPanel.Populate(testData);
        Assert.AreEqual("outcome", scoreboardPanel.resultText.text.ToLower());
    }

    [Test]
    public void PopulateFinalScore()
    {
        scoreboardPanel.Populate(testData);
        Assert.AreEqual("101", scoreboardPanel.finalScoreText.text.ToLower());
    }

    [Test]
    public void PopulateAchievements()
    {
        scoreboardPanel.Populate(testData);
        Assert.AreEqual(1, scoreboardPanel.achievementPanel.transform.childCount);
    }

    [Test]
    public void AchivementButtonToggle()
    {
        bool originalState = scoreboardPanel.achievementPanel.activeSelf;
        scoreboardPanel.OnShowAchievementsButton();
        Assert.AreEqual(!originalState, scoreboardPanel.achievementPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator ContinueLoadsMainMenu()
    {
        scoreboardPanel.OnContinueButton();
        while (GameManager.Instance.CurrentScene == inGameScenes.Loader)
        {
            yield return null;
        }
        Assert.AreEqual(inGameScenes.MainMenu, GameManager.Instance.CurrentScene);
    }
}
