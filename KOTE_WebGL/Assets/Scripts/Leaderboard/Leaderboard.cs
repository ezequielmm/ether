using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace.Leaderboard.New;
using Root = DefaultNamespace.Leaderboard.New.Root;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private LeaderboardListItem itemPrefab;
    [SerializeField] private Transform parent;
    private List<LeaderboardListItem> items = new();
    public void Show(bool show)
    {   
        gameObject.SetActive(show);
        if(show)
        {
            RequestLeaderboard();
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F8))
            RequestLeaderboard();
#endif
    }

    public async void RequestLeaderboard()
    {
        var baseUrl = /*WebRequesterManager.Instance.ConstructUrl(RestEndpoint.Leaderboard)*/ "https://api.dev.knightsoftheether.com/v1/leaderboard";
        Dictionary<string, string> parameters = new Dictionary<string, string>(){
            { "startDate", "2023-05-01T00%3A00%3A00Z" },
            { "endDate", "2023-06-01T00%3A00%3A00Z" },
            { "pageSize", "1000" },
            { "page", "1" },
            { "onlyWin", "false" }
        };
        string url = GetRequestUrl(baseUrl, parameters);
        
        using (var request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Error: {request.error}");
                return;
            }

            var leaderboard = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            Populate(leaderboard.data);
        }
    }
    
    private string GetRequestUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        string url = baseUrl;
        if (parameters != null && parameters.Count > 0)
        {
            url += "?";
            foreach (var parameter in parameters)
            {
                url += $"{parameter.Key}={parameter.Value}&";
            }
            url = url.Remove(url.Length - 1);
        }
        return url;
    }

    private void Populate(DataItem[] leaderboard)
    {
        // Clear
        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }
        items.Clear();
        
        // Populate
        for (int i = 0; i < leaderboard.Length; i++)
        {
            var player = leaderboard[i];
            var item = Instantiate(itemPrefab, parent);
            item.SetData(player, i % 2 == 0, i + 1);
            items.Add(item);
        }
    }
}
