using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace.Leaderboard.New;
using Root = DefaultNamespace.Leaderboard.New.Root;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private LeaderboardListItem itemPrefab;
    [SerializeField] private Transform parent;
    
    private List<LeaderboardListItem> items = new();

    public int page = 1;
    public int pageSize = 10;

    public UnityEvent<bool> OnLeaderboardShow;
    public UnityEvent<bool> OnLeaderboardReceived;
    
    public void Show(bool show)
    {   
        gameObject.SetActive(show);
        if(show)
        {
            RequestLeaderboard();
        }
        OnLeaderboardShow?.Invoke(show);
    }
    
    public void NextPage()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        page++;
        RequestLeaderboard();
    }
    
    public void PreviousPage()
    {
        GameManager.Instance.EVENT_PLAY_SFX.Invoke(SoundTypes.UI, "Button Click");
        page--;
        if (page < 1)
            page = 1;
        RequestLeaderboard();
    }
    
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F8))
            RequestLeaderboard();
#endif
    }

    public void GoFirstPage()
    {
        page = 1;
        RequestLeaderboard();
    }
    
    public async void GoLastPage()
    {
        var url = GetRequestUrl();
        
        using (var request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Error: {request.error}");
                OnLeaderboardReceived?.Invoke(false);
                return;
            }

            var leaderboard = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            page = leaderboard.pagination.totalPages;
            RequestLeaderboard();
        }
    }

    public async void RequestLeaderboard()
    {
        Debug.Log($"Requesting leaderboard at page: {page}");
        var url = GetRequestUrl();
        
        using (var request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Error: {request.error}");
                OnLeaderboardReceived?.Invoke(false);
                return;
            }

            var leaderboard = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            Populate(leaderboard.data);
            OnLeaderboardReceived?.Invoke(true);
        }
    }

    private string GetRequestUrl()
    {
        var baseUrl = /*WebRequesterManager.Instance.ConstructUrl(RestEndpoint.Leaderboard)*/ "https://api.dev.knightsoftheether.com/v1/leaderboard";
        Dictionary<string, string> parameters = new Dictionary<string, string>(){
            { "startDate", "2022-05-01T00%3A00%3A00Z&end[…]06-01T00%3A00%3A00Z" },
            { "pageSize", $"{pageSize}" },
            { "page", $"{page}" },
            { "onlyWin", "false" }
        };
        
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
