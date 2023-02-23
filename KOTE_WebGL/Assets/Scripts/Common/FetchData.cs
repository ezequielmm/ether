using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FetchData : ISingleton<FetchData>, IDisposable
{
    private static FetchData instance;
    public static FetchData Instance 
    { 
        get 
        {
            if (instance == null) 
            {
                instance = new FetchData();
            }
            return instance;
        } 
    }
    WebRequesterManager webRequest;
    WebSocketManager socketRequest;

    private FetchData() 
    {
        GameManager.Instance.EVENT_SCENE_LOADED.AddListener(OnSceneLoaded);
        OnSceneLoaded(GameManager.Instance.CurrentScene);
    }

    private void OnSceneLoaded(inGameScenes scene) 
    {
        switch(scene) 
        {
            case inGameScenes.MainMenu:
                webRequest = GameObject.FindObjectOfType<WebRequesterManager>();
                break;
            case inGameScenes.Expedition:
                socketRequest = WebSocketManager.Instance;
                break;
        }
    }

    public void DestroyInstance()
    {
        instance = null;
    }
    public void Dispose()
    {
        
    }

    public async UniTask<string> GetServerVersion()
    {
        string requestUrl = webRequest.ConstructUrl(RestEndpoint.ServerVersion);
        using (UnityWebRequest request = UnityWebRequest.Get(requestUrl))
        {
            string json = await webRequest.MakeRequest(request);
            var versionWrapper = JsonConvert.DeserializeObject<ServerVersionWrapper>(json);
            return versionWrapper.Version;
        }
    }
    [Serializable]
    private class ServerVersionWrapper
    {
        [JsonProperty("data")]
        public string Version;
    }

    public async UniTask<Deck> GetCardUpgradePair(string cardId) 
    {
        string json = await socketRequest.EmitAwaitResponse(SocketEvent.GetCardUpgradePair, cardId);
        SWSM_DeckData deckData = JsonConvert.DeserializeObject<SWSM_DeckData>(json);
        Deck deck = new Deck() { cards = deckData.data.data.deck };
        return deck;
    }
}
