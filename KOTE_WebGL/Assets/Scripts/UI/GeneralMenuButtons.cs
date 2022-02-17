using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralMenuButtons : MonoBehaviour //Only buttons to be used in multiple panels exactly the same way (perhaps make this class a parent class?)
{
    public void loadScene(inGameScenes inGameScenesVariable)
    {
        GameManager.Instance.LoadScene(inGameScenesVariable);
    }
    public void loadScene(EnumInvoker inGameScenesVariable)
    {
        GameManager.Instance.LoadScene(inGameScenesVariable.Scene);
    }
}
