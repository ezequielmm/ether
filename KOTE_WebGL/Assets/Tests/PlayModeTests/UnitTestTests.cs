using KOTE.UI.Armory;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SetUpFixture]
public class UnitTestTests : MonoBehaviour
{
    [Test]
    public void InUnitTestTrue()
    {
        Assert.IsTrue(UnitTestDetector.IsInUnitTest);
    }

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        RecordPlayerPrefs();
        UnitTestDetector.IsInUnitTest = true;
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        UnitTestDetector.IsInUnitTest = false;
        RecoverPlayerPrefs();
    }

    private Dictionary<string, string> StringPrefs = new();
    private Dictionary<string, int> IntPrefs = new();
    private Dictionary<string, float> FloatPrefs = new();
    private void RecordPlayerPrefs()
    {
        RecordStringPref("client_id");
        RecordStringPref("session_token");
        RecordIntPref("allIn1DefaultShader");
        RecordStringPref("email_reme_login");
        RecordStringPref("date_reme_login");
        RecordIntPref("enable_injured_idle");
        RecordStringPref("ws_url");
        RecordStringPref("api_url");
        RecordIntPref("enable_registration");
        RecordIntPref("enable_royal_house");
        RecordIntPref("enable_node_numbers");
        RecordFloatPref("settings_volume");
        RecordFloatPref("sfx_volume");
        RecordFloatPref("music_volume");
        RecordIntPref("settings_dropdown");
        
        PlayerPrefs.DeleteAll();
    }
    private void RecordStringPref(string pref)
    {
        StringPrefs.Add(pref, PlayerPrefs.GetString(pref));
    }
    private void RecordIntPref(string pref)
    {
        IntPrefs.Add(pref, PlayerPrefs.GetInt(pref));
    }
    private void RecordFloatPref(string pref)
    {
        FloatPrefs.Add(pref, PlayerPrefs.GetFloat(pref));
    }

    private void RecoverPlayerPrefs()
    {
        foreach (var kvp in StringPrefs)
        {
            PlayerPrefs.SetString(kvp.Key, kvp.Value);
        }
        foreach (var kvp in IntPrefs)
        {
            PlayerPrefs.SetInt(kvp.Key, kvp.Value);
        }
        foreach (var kvp in FloatPrefs)
        {
            PlayerPrefs.SetFloat(kvp.Key, kvp.Value);
        }
        PlayerPrefs.Save();
    }
}
