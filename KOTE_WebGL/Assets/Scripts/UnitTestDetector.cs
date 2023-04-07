#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Detect if we are running as part of a nUnit unit test.
/// This is DIRTY and should only be used if absolutely necessary 
/// as its usually a sign of bad design.
/// </summary>    
static class UnitTestDetector
{
    static UnitTestDetector()
    {
        if (Environment.StackTrace.Contains("UnityEngine.TestRunner"))
        {
            IsInUnitTest = true;
        }
    }

    public static bool IsInUnitTest = false;
}
#endif