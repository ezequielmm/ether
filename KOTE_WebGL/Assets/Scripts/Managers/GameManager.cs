using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum inGameScenes
{
    MainMenu,
    Loader
}; //created so we can use the names on the enums instead of hard coding strings everytime, if a scene name is changed we can just change it here as well instead of changing at various spots

public class GameManager : SingleTon<GameManager>
{
    public UnityEvent<bool, string> EVENT_REQUEST_NAME = new UnityEvent<bool, string>();
    public UnityEvent<string> EVENT_NEW_RANDOM_NAME = new UnityEvent<string>();

    public UnityEvent<string, string, string> EVENT_REGISTER = new UnityEvent<string, string, string>();

    public UnityEvent<string, string, string, string> EVENT_REGISTER_COMPLETED =
        new UnityEvent<string, string, string, string>();

    public UnityEvent<string, string, bool> EVENT_LOGIN = new UnityEvent<string, string, bool>();
    public UnityEvent<string, string, int, bool> EVENT_LOGIN_COMPLETED = new UnityEvent<string, string, int, bool>();

    public UnityEvent<string> EVENT_PROFILE = new UnityEvent<string>();
    public UnityEvent<string, int> EVENT_PROFILE_REQUESTED = new UnityEvent<string, int>();

    public UnityEvent<bool> EVENT_LOGINPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();
    public UnityEvent<bool> EVENT_REGISTERPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    public inGameScenes
        nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    // Start is called before the first frame update
    void Start()
    {
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        SceneManager.LoadScene(inGameScenes.Loader.ToString());
        nextSceneToLoad = scene;
    }
}