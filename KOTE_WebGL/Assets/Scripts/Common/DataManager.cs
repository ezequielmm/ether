using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataManager
{
    protected WebRequesterManager webRequest;
    protected WebSocketManager socketRequest;

    protected DataManager()
    {
        GameManager.Instance.EVENT_SCENE_LOADED.AddListener(OnSceneLoaded);
        OnSceneLoaded(GameManager.Instance.CurrentScene);
    }

    private void OnSceneLoaded(inGameScenes scene)
    {
        switch (scene)
        {
            case inGameScenes.MainMenu:
                webRequest = GameObject.FindObjectOfType<WebRequesterManager>();
                break;
            case inGameScenes.Expedition:
                socketRequest = WebSocketManager.Instance;
                break;
        }
    }
}
