using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class DebugManagerTests : MonoBehaviour
{
    private DebugManager debugManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        GameObject hiddenConsolePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/Console.prefab");
        GameObject debugManagerInstance = Instantiate(hiddenConsolePrefab);
        debugManager = debugManagerInstance.GetComponent<DebugManager>();
        debugManagerInstance.SetActive(true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Destroy(debugManager.gameObject);
        yield return null;
    }

    [Test]
    public void DoesStartDeactivateConsole()
    {
        Assert.False(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesDisableDebugSwitchFilterLogTypeToException()
    {
        Assert.AreEqual(LogType.Log, Debug.unityLogger.filterLogType);
        DebugManager.DisableDebug();
        Assert.AreEqual(LogType.Exception, Debug.unityLogger.filterLogType);
    }

    [Test]
    public void DoesEnableDebugSwitchFilterLogTypeToLog()
    {
        DebugManager.DisableDebug();
        Assert.AreEqual(LogType.Exception, Debug.unityLogger.filterLogType);
        DebugManager.EnableDebug();
        Assert.AreEqual(LogType.Log, Debug.unityLogger.filterLogType);
    }

    [Test]
    public void DoesShowConsoleActivateConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        Assert.True(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesPublicLogLogMessage()
    {
        DebugManager.PublicLog("this is a log");
        LogAssert.Expect(LogType.Log, "this is a log");
    }

    [Test]
    public void DoesSendingUnknownCommandLogTheProblem()
    {
        debugManager.OnTextInput("test");
        LogAssert.Expect(LogType.Log, "Unknown Command.");
    }

    [Test]
    public void DoesSendingWsUrlCommandLogUrl()
    {
        PlayerPrefs.SetString("ws_url", "testurl");
        debugManager.OnTextInput("ws_url");
        LogAssert.Expect(LogType.Log, "testurl");
    }

    [Test]
    public void DoesSendingWsUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("ws_url", "");
        debugManager.OnTextInput("ws_url");
        LogAssert.Expect(LogType.Log, "Websocket not yet connected.");
    }

    [Test]
    public void DoesSendingApiUrlCommandLogApiUrl()
    {
        PlayerPrefs.SetString("api_url", "urlTest");
        debugManager.OnTextInput("api_url");
        LogAssert.Expect(LogType.Log, "urlTest");
    }

    [Test]
    public void DoesSendingApiUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("api_url", "");
        debugManager.OnTextInput("api_url");
        LogAssert.Expect(LogType.Log, "No API URL found.");
    }

    [Test]
    public void DoesSendingApisUrlCommandLogApiUrl()
    {
        PlayerPrefs.SetString("api_url", "urlTest");
        debugManager.OnTextInput("apis_url");
        LogAssert.Expect(LogType.Log, "urlTest");
    }

    [Test]
    public void DoesSendingApisUrlCommandLogNotConnectedIfEmptyUrl()
    {
        PlayerPrefs.SetString("api_url", "");
        debugManager.OnTextInput("apis_url");
        LogAssert.Expect(LogType.Log, "No API URL found.");
    }

    [Test]
    public void DoesSendingPlayerTokenCommandLogPlayerToken()
    {
        PlayerPrefs.SetString("session_token", "ArandomToken");
        debugManager.OnTextInput("player_token");
        LogAssert.Expect(LogType.Log, "ArandomToken");
    }

    [Test]
    public void DoesSendingPlayerTokenCommandLogNoTokenGenerated()
    {
        PlayerPrefs.SetString("session_token", "");
        debugManager.OnTextInput("player_token");
        LogAssert.Expect(LogType.Log, "No token has been generated yet.");
    }

    [Test]
    public void DoesSendingQuitCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        debugManager.OnTextInput("quit");
        Assert.False(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingExitCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        debugManager.OnTextInput("exit");
        Assert.False(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingCloseCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        debugManager.OnTextInput("close");
        Assert.False(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingCloseConsoleCloseHiddenConsole()
    {
        GameManager.Instance.EVENT_SHOW_CONSOLE.Invoke();
        debugManager.OnTextInput("close_console");
        Assert.False(debugManager.consoleContainer.activeSelf);
    }

    [Test]
    public void DoesSendingDisableDebugDisableDebug()
    {
        DebugManager.EnableDebug();
        debugManager.OnTextInput("disable_debug");
        Assert.AreEqual(LogType.Exception, Debug.unityLogger.filterLogType);
        LogAssert.Expect(LogType.Log, "Console Disabled.");
    }

    [Test]
    public void DoesSendingEnableDebugEnableDebug()
    {
        DebugManager.DisableDebug();
        debugManager.OnTextInput("enable_debug");
        Assert.AreEqual(LogType.Log, Debug.unityLogger.filterLogType);
        LogAssert.Expect(LogType.Log, "Console Enabled.");
    }

    [Test]
    public void DoesSendingVersionLogCurrentVersion()
    {
        debugManager.OnTextInput("version");
        LogAssert.Expect(LogType.Log, $"{Application.version}");
    }

    [Test]
    public void DoesSendingShowCommandsShowAllCommands()
    {
        debugManager.OnTextInput("show_commands");
        LogAssert.Expect(LogType.Log,
            "Available commands: ws_url, apis_url, api_url, player_token, quit, exit, close, close_console," +
            " enable_debug, disable_debug, version, show_commands, help, enable_all_functionality, " +
            "enable_register_panel, enable_armory_panel, enable_royal_houses_panel, enable_node_numbers, " +
            "disable_all_functionality, disable_register_panel, disable_armory_panel, disable_royal_houses_panel," +
            " disable_node_numbers");
    }
}