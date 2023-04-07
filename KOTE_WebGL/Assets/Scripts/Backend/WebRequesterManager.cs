using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// Check HelperClasses.cs for the classes usaed to hold JSON data
/// </summary>
public class WebRequesterManager : SingleTon<WebRequesterManager>
{
    private string baseUrl => ClientEnvironmentManager.Instance.WebRequestURL;

    protected override void Awake()
    {
        base.Awake();

        if (this != Instance) return;

        PlayerPrefs.SetString("api_url", baseUrl);

        GameManager.Instance.EVENT_SEND_BUG_FEEDBACK.AddListener(SendBugReport);
    }

    public void RequestCharacterList()
    {
        StartCoroutine(GetCharacterList());
    }

    public async UniTask<DownloadHandler> MakeRequest(UnityWebRequest request)
    {
#if UNITY_EDITOR
        if (UnitTestDetector.IsInUnitTest)
        {
            Debug.Log($"[WebRequesterManager] Can't make a webrequest while testing.");
            return null;
        }
#endif
        try
        {
            Guid requestId = Guid.NewGuid();
            LogRequest(requestId, request.uri.ToString());
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception();
            }
            else
            {
                bool logText = IsResponseJson(request) && request?.downloadHandler?.text != null;
                LogRepsonse(requestId, request.uri.ToString(),
                    logText
                        ? request.downloadHandler.text
                        : $"<Cannot Display: {request.GetResponseHeader("Content-Type")}>");
                return request.downloadHandler;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogError(
                $"[WebRequesterManager] Error sending [{request.method}] request to [{request.uri}]\n{request?.error}");
            ServerCommunicationLogger.Instance.LogCommunication(
                $"[{request.method}][{request.uri}] Data Not Retrieved: {request?.error}",
                CommunicationDirection.Incoming);
            return null;
        }
    }

    List<string> jsonCarryingFormats = new List<string>() { "application/json", "text/plain", "text/json" };

    private bool IsResponseJson(UnityWebRequest response)
    {
        string responseType = response.GetResponseHeader("Content-Type")?.ToLower().Split(';')?
            .GetValue(0)?.ToString() ?? string.Empty;
        return jsonCarryingFormats.Contains(responseType);
    }

    private void LogRequest(Guid requestId, string url, params object[] payload)
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
        string variableString = LogHelper.VariablesToHumanReadable(url, payload);
        string jsonString = LogHelper.VariablesToJson(url, payload);
        Debug.Log($"[WebRequesterManager] REQUEST [{requestIdShortened}] >>> {jsonString}");
        LogHelper.SendOutgoingCommunicationLogs(
            $"[WebRequesterManager] REQUEST [{requestIdShortened}] >>> {variableString}", jsonString);
    }

    private void LogRepsonse(Guid requestId, string url, string rawJson)
    {
        string requestIdShortened = requestId.ToString().Substring(0, LogHelper.LengthOfIdToLog);
        string variableString = LogHelper.VariablesToHumanReadable(url, rawJson);
        Debug.Log($"[WebRequesterManager] RESPONSE [{requestIdShortened}] <<< {rawJson}");
        LogHelper.SendOutgoingCommunicationLogs(
            $"[WebRequesterManager] RESPONSE [{requestIdShortened}] <<< {variableString}", rawJson);
    }

    IEnumerator GetCharacterList()
    {
        string token = AuthenticationManager.Instance.GetSessionToken();

        Debug.Log("[GetCharacterList] with token " + token);
        ServerCommunicationLogger.Instance.LogCommunication("Character list request. token: " + token,
            CommunicationDirection.Outgoing);

        string fullUrl = $"{baseUrl}{RestEndpoint.CharactersList}";
        WWWForm form = new WWWForm();

        using (UnityWebRequest request = UnityWebRequest.Get($"{fullUrl}?Authorization={Uri.EscapeDataString(token)}"))
        {
            // using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form);
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Error getting GetCharacterList status] " + request.error);
                ServerCommunicationLogger.Instance.LogCommunication(
                    "Character list error: " + request.error, CommunicationDirection.Incoming);

                yield break;
            }

            Debug.Log("answer from GetCharacterList status " + request.downloadHandler.text);
            ServerCommunicationLogger.Instance.LogCommunication(
                "Character list success: " + request.downloadHandler.text, CommunicationDirection.Incoming);


            //LogoutData answerData = JsonConvert.DeserializeObject<LogoutData>(request.downloadHandler.text);
            //string message = logoutData.data.message;

            //TODO: check for errors even on sucessful result
        }
    }

    private async void SendBugReport(string title, string description, string base64Image)
    {
        string fullUrl = $"{baseUrl}{RestEndpoint.BugReport}";

        BugReportData reportData = new BugReportData
        {
            reportId = Guid.NewGuid().ToString(),
            environment = ClientEnvironmentManager.Instance.Environment.ToString(),
            clientId = UserDataManager.Instance.ClientId,
            account = UserDataManager.Instance.UserEmail,
            knightId = UserDataManager.Instance.ActiveNft,
            expeditionId = UserDataManager.Instance.ExpeditionId,
            userTitle = title,
            userDescription = description,
            screenshot = base64Image,
            frontendVersion = VersionManager.ClientVersionWithCommit,
            backendVersion = VersionManager.ServerVersion,
            messageLog = ServerCommunicationLogger.Instance.GetCommunicationLog()
        };
        string data = JsonConvert.SerializeObject(reportData);
        byte[] utf8String = Encoding.Default.GetBytes(data);
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            var uploadHandler = new UploadHandlerRaw(utf8String);
            uploadHandler.contentType = $"application/json";
            request.uploadHandler = uploadHandler;
            await MakeRequest(request);
            uploadHandler.Dispose();
        }

        Debug.Log($"[WebRequesterManager] Bug Report Sent!");
    }

    public string ConstructUrl(string path)
    {
        string host = ClientEnvironmentManager.Instance.WebRequestURL;
      
        return $"{host}{path}";
    }
}


public static class RestEndpoint
{
    public static readonly string RandomName = "/auth/v1/generate/username";
    public static readonly string Register = "/auth/v1/register";
    public static readonly string Login = "/auth/v1/login";
    public static readonly string Logout = "/auth/v1/logout";

    public static readonly string Profile = "/gsrv/v1/profile";

    public static readonly string WalletData = "/gsrv/v1/wallets";
    public static readonly string VerifyWalletSignature = "/gsrv/v1/tokens/verify";
    public static readonly string CharactersList = "/gsrv/v1/characters";
    public static readonly string ExpeditionStatus = "/gsrv/v1/expeditions/status";
    public static readonly string ExpeditionRequest = "/gsrv/v1/expeditions";
    public static readonly string ExpeditionCancel = "/gsrv/v1/expeditions/cancel";
    public static readonly string ExpeditionScore = "/gsrv/v1/expeditions/score";
    public static readonly string BugReport = "/gsrv/v1/bug/report";
    public static readonly string ServerVersion = "/gsrv/v1/showversion";
    public static readonly string PlayerGear = "/gsrv/v1/playergear";
    public static readonly string CurrentContest = "/gsrv/v1/showcontest";
}