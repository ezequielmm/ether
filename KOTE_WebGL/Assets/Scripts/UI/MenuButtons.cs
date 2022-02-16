using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] GameManager GameManagerLocal;
    // Start is called before the first frame update
    void Start()
    {
        GameManagerLocal = GameManager._instance.gameObject.GetComponent<GameManager>();
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
