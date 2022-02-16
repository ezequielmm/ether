using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum inGameScenes {MainMenuScene,LoaderScene,Map,Game}; //created so we can use the names on the enums instead of hard coding strings everytime, if a scene name is changed we can just change it here as well instead of changing at various spots
public enum connectionState {LoggedOut, Loggedin};
public class GameManager : SingleTon
{

    public inGameScenes nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat
    [SerializeField] // permits changes on editor
    connectionState currentConnectionState = connectionState.LoggedOut; // used to keep track of log in state, maybe can be changed to a bool but might be less safe
    [SerializeField]
    string username;
    protected override void Awake()
    {
        base.Awake();
        //nextSceneToLoad = inGameScenes.Map; 
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //LoadScene(default(inGameScenes)); locks into the load scene(debug test)
        //LoadScene(inGameScenes.Map);//setting the first scene to map for now, will be MainMenuScene when that is prepared (unecessary as there is a demo MainMenuScene)
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        SceneManager.LoadScene(inGameScenes.LoaderScene.ToString());
        nextSceneToLoad = scene;
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(inGameScenes.LoaderScene.ToString()); //Loads the Main Menu scene passing through the LoaderScene
        nextSceneToLoad = default(inGameScenes);
    }

    public connectionState CheckConnection()
    {
        return currentConnectionState;
    }
    public void LogIn()
    {
        currentConnectionState = connectionState.Loggedin;
    }
    public void LogOut()
    {
        currentConnectionState = connectionState.LoggedOut;
    }

}
