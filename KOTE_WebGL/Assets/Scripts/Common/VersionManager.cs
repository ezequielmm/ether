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
    /// <summary>
    /// Client Version number plus commit hash if avalible
    /// </summary>
    public static string ClientVersionWithCommit => $"{ClientVersion}{(string.IsNullOrEmpty(ClientCommitHash) ? "" : $"+{ClientCommitHash.Substring(0, 7)}")}";
    public static string ServerVersion => GameManager.ServerVersion ?? string.Empty;
    private string ClientId => UserDataManager.Instance.ClientId;
    private string Environment => ClientEnvironmentManager.Instance.Environment.ToString().Substring(0, 3).ToLower();



    // Start is called before the first frame update
    void Start()
    {
        SetTMP();
        GameManager.Instance.EVENT_VERSION_UPDATED.AddListener(SetTMP);
    }

    private void SetTMP() 
    {
        clientInfo.text = this.ToString();
    }

    public override string ToString() 
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Server Version: v{ServerVersion}");
        sb.AppendLine(System.Environment.NewLine);
        sb.Append($"Client Version: {Environment}{ClientVersionWithCommit}");
        sb.AppendLine(System.Environment.NewLine);
        sb.Append($"Client Id: {ClientId}");

        return sb.ToString();
    }
}
