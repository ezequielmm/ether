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
}
