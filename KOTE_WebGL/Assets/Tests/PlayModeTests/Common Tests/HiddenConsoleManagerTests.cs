using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class HiddenConsoleManagerTests : MonoBehaviour
{
    private HiddenConsoleManager HiddenConsoleManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject hiddenConsolePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/Console.prefab");
        GameObject HiddenConsoleManagerInstance = Instantiate(hiddenConsolePrefab);
        HiddenConsoleManager = HiddenConsoleManagerInstance.GetComponent<HiddenConsoleManager>();
        HiddenConsoleManagerInstance.SetActive(true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(HiddenConsoleManager.gameObject);
        GameManager.Instance.DestroyInstance();
        yield return null;
    }

    [Test]
    public void DoesStartDeactivateConsole()
    {
        Assert.False(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesDisableDebugSwitchFilterLogTypeToException()
    {
        Assert.AreEqual(LogType.Log, GameSettings.FilterLogType);
        HiddenConsoleManager.DisableDebug();
        Assert.AreEqual(LogType.Exception, GameSettings.FilterLogType);
    }

    [Test]
    public void DoesEnableDebugSwitchFilterLogTypeToLog()
    {
        HiddenConsoleManager.DisableDebug();
        Assert.AreEqual(LogType.Exception, GameSettings.FilterLogType);
        HiddenConsoleManager.EnableDebug();
        Assert.AreEqual(LogType.Log, GameSettings.FilterLogType);
    }

    [Test]
    public void DoesShowConsoleActivateConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        Assert.True(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesPublicLogLogMessage()
    {
        HiddenConsoleManager.PublicLog("this is a log");
        LogAssert.Expect(LogType.Log, "this is a log");
    }

    [Test]
    public void DoesSendingUnknownCommandLogTheProblem()
    {
        HiddenConsoleManager.OnTextInput("test");
        LogAssert.Expect(LogType.Log, "Unknown Command.");
    }

    [Test]
    public void DoesSendingWsUrlCommandLogUrl()
    {
        PlayerPrefs.SetString("ws_url", "testurl");
        HiddenConsoleManager.OnTextInput("ws_url");
        LogAssert.Expect(LogType.Log, "testurl");
    }

    [Test]
    public void DoesSendingWsUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("ws_url", "");
        HiddenConsoleManager.OnTextInput("ws_url");
        LogAssert.Expect(LogType.Log, "Websocket not yet connected.");
    }

    [Test]
    public void DoesSendingApiUrlCommandLogApiUrl()
    {
        PlayerPrefs.SetString("api_url", "urlTest");
        HiddenConsoleManager.OnTextInput("api_url");
        LogAssert.Expect(LogType.Log, "urlTest");
    }

    [Test]
    public void DoesSendingApiUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("api_url", "");
        HiddenConsoleManager.OnTextInput("api_url");
        LogAssert.Expect(LogType.Log, "No API URL found.");
    }

    [Test]
    public void DoesSendingApisUrlCommandLogApiUrl()
    {
        PlayerPrefs.SetString("api_url", "urlTest");
        HiddenConsoleManager.OnTextInput("apis_url");
        LogAssert.Expect(LogType.Log, "urlTest");
    }

    [Test]
    public void DoesSendingApisUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("api_url", "");
        HiddenConsoleManager.OnTextInput("apis_url");
        LogAssert.Expect(LogType.Log, "No API URL found.");
    }

    [Test]
    public void DoesSendingPlayerTokenCommandLogPlayerToken()
    {
        PlayerPrefs.SetString("session_token", "ArandomToken");
        PlayerPrefs.SetString("login_time", DateTime.UtcNow.ToString());
        HiddenConsoleManager.OnTextInput("player_token");
        LogAssert.Expect(LogType.Log, "ArandomToken");
    }

    [Test]
    public void DoesSendingPlayerTokenCommandLogNoTokenGenerated()
    {
        PlayerPrefs.SetString("session_token", "");
        HiddenConsoleManager.OnTextInput("player_token");
        LogAssert.Expect(LogType.Log, "No token has been generated yet.");
    }

    [Test]
    public void DoesSendingQuitCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        HiddenConsoleManager.OnTextInput("quit");
        Assert.False(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingExitCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        HiddenConsoleManager.OnTextInput("exit");
        Assert.False(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingCloseCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        HiddenConsoleManager.OnTextInput("close");
        Assert.False(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingCloseConsoleCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        HiddenConsoleManager.OnTextInput("close_console");
        Assert.False(HiddenConsoleManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingEnableDebugEnableDebug()
    {
        HiddenConsoleManager.DisableDebug();
        HiddenConsoleManager.OnTextInput("enable_debug");
        Assert.AreEqual(LogType.Log, Debug.unityLogger.filterLogType);
        LogAssert.Expect(LogType.Log, "Console Enabled.");
    }

    [Test]
    public void DoesSendingVersionLogCurrentVersion()
    {
        HiddenConsoleManager.OnTextInput("version");
        LogAssert.Expect(LogType.Log, $"{Application.version}");
    }

    [Test]
    public void DoesSendingShowCommandsShowAllCommands()
    {
        HiddenConsoleManager.OnTextInput("show_commands");
        LogAssert.Expect(LogType.Log,
            "Available commands: ws_url, apis_url, api_url, player_token, quit, exit, close, close_console," +
            " enable_debug, disable_debug, version, show_commands, help, enable_all_functionality," +
            " enable_register_panel, enable_armory_panel, enable_royal_houses_panel, enable_node_numbers," +
            " disable_all_functionality, disable_register_panel, disable_armory_panel, disable_royal_houses_panel," +
            " disable_node_numbers, enable_node, enable_injured_idle, disable_injured_idle," +
            " reset_all, use_nft, get_score, commit, environment");
    }
}