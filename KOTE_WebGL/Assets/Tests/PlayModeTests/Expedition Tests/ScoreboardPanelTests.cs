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

    [UnityTest]
    public IEnumerator PanelToggleOn()
    {
        scoreboardPanel.TogglePanel(true);
        yield return null;
        Assert.AreEqual(true, scoreboardPanel.gameOverContainer.activeSelf);
    }

    [UnityTest]
    public IEnumerator PanelToggleOff()
    {
        scoreboardPanel.TogglePanel(false);
        yield return null;
        Assert.AreEqual(false, scoreboardPanel.gameOverContainer.activeSelf);
    }

    [UnityTest]
    public IEnumerator PopulateOutcome()
    {
        scoreboardPanel.Populate(testData);
        yield return null;
        Assert.AreEqual("outcome", scoreboardPanel.resultText.text.ToLower());
    }

    [UnityTest]
    public IEnumerator PopulateFinalScore()
    {
        scoreboardPanel.Populate(testData);
        yield return null;
        Assert.AreEqual("101", scoreboardPanel.finalScoreText.text.ToLower());
    }

    [UnityTest]
    public IEnumerator PopulateAchievements()
    {
        scoreboardPanel.Populate(testData);
        yield return null;
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
