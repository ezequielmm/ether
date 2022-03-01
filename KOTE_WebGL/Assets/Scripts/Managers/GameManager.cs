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
    public class EVENT_STRING_CLASS : UnityEvent<string>
    {
    }

    public UnityEvent<string> EVENT_NEW_RANDOM_NAME = new UnityEvent<string>();
    public UnityEvent<string> EVENT_REGISTER_TOKEN = new UnityEvent<string>();

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