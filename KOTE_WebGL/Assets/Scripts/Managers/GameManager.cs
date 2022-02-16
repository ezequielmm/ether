using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum inGameScenes {MainMenu,Loader,Map,Game}; //created so we can use the names on the enums instead of hard coding strings everytime, if a scene name is changed we can just change it here as well instead of changing at various spots

public class GameManager : SingleTon<GameManager>
{

    public inGameScenes nextSceneToLoad; // maybe we can encapsulate this variable to control who can set it and allow all to get the value? Depending on the scene that is loaded there might be a change for a cheat

    // Start is called before the first frame update
    void Start()
    {
        //LoadScene(default(inGameScenes)); locks into the load scene(debug test)
        LoadScene(inGameScenes.MainMenu);//setting the first scene to map for now, will be MainMenuScene when that is prepared (unecessary as there is a demo MainMenuScene)
    }

    public void LoadScene(inGameScenes scene) //Loads the target scene passing through the LoaderScene
    {
        SceneManager.LoadScene(inGameScenes.Loader.ToString());
        nextSceneToLoad = scene;
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(inGameScenes.Loader.ToString()); //Loads the Main Menu scene passing through the LoaderScene
        nextSceneToLoad = default(inGameScenes);
    }

}
