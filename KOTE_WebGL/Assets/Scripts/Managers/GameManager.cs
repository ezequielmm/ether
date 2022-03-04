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


    //REGISTER ACCOUNT EVENTS
    public UnityEvent<string, string, string> EVENT_REQUEST_REGISTER = new UnityEvent<string, string, string>();
    public UnityEvent<string> EVENT_REQUEST_REGISTER_ERROR = new UnityEvent<string>();

    public UnityEvent<bool> EVENT_REGISTERPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    public UnityEvent<string> EVENT_REQUEST_NAME = new UnityEvent<string>();
    public UnityEvent<string> EVENT_REQUEST_NAME_SUCESSFUL = new UnityEvent<string>();
    public UnityEvent<string> EVENT_REQUEST_NAME_ERROR = new UnityEvent<string>();

    //LOGIN EVENTS

    public UnityEvent<string, string> EVENT_REQUEST_LOGIN = new UnityEvent<string, string>();   
    public UnityEvent<string, int> EVENT_REQUEST_LOGIN_SUCESSFUL = new UnityEvent<string, int>();
    public UnityEvent<string> EVENT_REQUEST_LOGIN_ERROR = new UnityEvent<string>();

    public UnityEvent<bool> EVENT_LOGINPANEL_ACTIVATION_REQUEST = new UnityEvent<bool>();

    public UnityEvent<string> EVENT_REQUEST_PROFILE = new UnityEvent<string>();  
    public UnityEvent<string> EVENT_REQUEST_PROFILE_ERROR = new UnityEvent<string>();   
   

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