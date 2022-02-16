using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralMenuButtons : MonoBehaviour //Only buttons to be used in multiple panels exactly the same way (perhaps make this class a parent class?)
{
    [SerializeField] GameManager GameManagerLocal;
    // Start is called before the first frame update
    void Start()
    {
        GameManagerLocal = GameManager.Instance;
    }

    public void loadScene(inGameScenes inGameScenesVariable)
    {
        GameManagerLocal.LoadScene(inGameScenesVariable);
    }
    public void loadScene(EnumInvoker inGameScenesVariable)
    {
        GameManagerLocal.LoadScene(inGameScenesVariable.Scene);
    }
    public void loadScene()
    {
        GameManagerLocal.LoadScene();
    }
}
