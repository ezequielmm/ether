using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class VersionManager : MonoBehaviour
{
    public TMP_Text clientInfo;

    public static string ClientVersion => Application.version;
    public static string ClientCommitHash => GitCommitHash.CommitHash;
    public static string ClientVersionFormatted => $"{ClientVersion}{(string.IsNullOrEmpty(ClientCommitHash) ? "" : $"+{ClientCommitHash.Substring(0, 7)}")}";
    public static string ServerVersion => GameManager.ServerVersion;
    public static string ClientId => GameManager.ClientId;
    public static string Environment => GameManager.ClientEnvironment;


    // Start is called before the first frame update
    void Start()
    {
        SetTMP();
        GameManager.Instance.EVENT_VERSION_UPDATED.AddListener(SetTMP);
    }

    private void SetTMP() 
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Server Version: v{ServerVersion}");
        sb.AppendLine(System.Environment.NewLine);
        sb.Append($"Client Version: {Environment.Substring(0, 3).ToLower()}{ClientVersionFormatted}");
        sb.AppendLine(System.Environment.NewLine);
        sb.Append($"Client Id: {ClientId}");

        clientInfo.text = sb.ToString();
    }
}
