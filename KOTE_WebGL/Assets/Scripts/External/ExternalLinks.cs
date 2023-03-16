using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ExternalLinks
{
    public static void OpenLink(string url)
    {
        openWindow(url);
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
}
